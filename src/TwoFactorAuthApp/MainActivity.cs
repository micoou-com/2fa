using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using TwoFactorAuth.Core.Data;
using TwoFactorAuth.Core.Totp;

namespace TwoFactorAuthApp;

[Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/AppTheme")]
public class MainActivity : AppCompatActivity
{
    private const int RequestScan = 42;
    private JsonAccountStore? _store;
    private AccountAdapter? _adapter;
    private readonly Handler _handler = new(Looper.MainLooper!);
    private readonly TickRunnable _tickRunnable;
    private bool _tickRunning;

    public MainActivity() => _tickRunnable = new TickRunnable(this);

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_main);

        string dataPath = Path.Combine(FilesDir!.AbsolutePath!, "accounts.json");
        _store = new JsonAccountStore(dataPath);
        var recycler = FindViewById<RecyclerView>(Resource.Id.recycler)!;
        recycler.SetLayoutManager(new LinearLayoutManager(this));
        _adapter = new AccountAdapter(_store, OnDeleteRequested);
        recycler.SetAdapter(_adapter);

        FindViewById<Android.Views.View>(Resource.Id.btnScan)!.Click += (_, _) => StartActivityForResult(
            new Intent(this, typeof(ScanActivity)), RequestScan);
        FindViewById<Android.Views.View>(Resource.Id.btnManual)!.Click += (_, _) => ShowManualDialog();
    }

    private void OnDeleteRequested(string id)
    {
        new AndroidX.AppCompat.App.AlertDialog.Builder(this!)
            .SetTitle(Resource.String.delete_confirm_title)
            .SetMessage(Resource.String.delete_confirm_message)
            .SetNegativeButton(Resource.String.btn_cancel, (s, e) => { })
            .SetPositiveButton(Resource.String.btn_ok, (s, e) =>
            {
                _store?.Remove(id);
                _adapter?.Reload();
            })
            .Show();
    }

    private void ShowManualDialog()
    {
        View? dialogView = LayoutInflater.From(this)!.Inflate(Resource.Layout.dialog_manual_add, null);
        var issuer = dialogView!.FindViewById<Google.Android.Material.TextField.TextInputEditText>(Resource.Id.edit_issuer)!;
        var secret = dialogView.FindViewById<Google.Android.Material.TextField.TextInputEditText>(Resource.Id.edit_secret)!;
        var secretLayout = dialogView.FindViewById<Google.Android.Material.TextField.TextInputLayout>(Resource.Id.layout_secret)!;

        var dialog = new AndroidX.AppCompat.App.AlertDialog.Builder(this!)
            .SetTitle(Resource.String.manual_title)
            .SetView(dialogView)
            .SetNegativeButton(Resource.String.btn_cancel, (s, e) => { })
            .SetPositiveButton(Resource.String.btn_ok, (s, e) => { })
            .Create();

        dialog!.Show();
        dialog.GetButton((int)Android.Content.DialogButtonType.Positive)!.Click += (_, _) =>
        {
            secretLayout.Error = null;
            string iss = issuer.Text?.ToString()?.Trim() ?? "";
            string sec = secret.Text?.ToString()?.Trim() ?? "";
            if (!TryAddFromManual(iss, sec, out string? err))
            {
                secretLayout.Error = err;
                return;
            }

            HideKeyboard(secret);
            dialog.Dismiss();
        };
    }

    private bool TryAddFromManual(string issuer, string secret, out string? errorMessage)
    {
        errorMessage = null;
        if (!SecretBytes.TryDecodeSecret(secret, out _, out string? err))
        {
            errorMessage = SecretBytes.DescribeDecodeError(err);
            return false;
        }

        string label = string.IsNullOrWhiteSpace(issuer) ? "Manual" : issuer;
        string storedSecret = SecretBytes.NormalizeSeparators(secret);
        _store?.Add(new AccountEntry
        {
            Issuer = label,
            AccountName = label,
            SecretBase32 = storedSecret,
            Digits = 6,
            PeriodSeconds = 30
        });
        _adapter?.Reload();
        return true;
    }

    private void HideKeyboard(Android.Views.View view)
    {
        var imm = (InputMethodManager?)GetSystemService(Context.InputMethodService!);
        imm?.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
    }

    protected override void OnStart()
    {
        base.OnStart();
        _adapter?.Reload();
        StartTick();
    }

    protected override void OnStop()
    {
        StopTick();
        base.OnStop();
    }

    private void StartTick()
    {
        if (_tickRunning)
            return;
        _tickRunning = true;
        _handler.PostDelayed(_tickRunnable, 200);
    }

    private void StopTick()
    {
        _tickRunning = false;
        _handler.RemoveCallbacks(_tickRunnable);
    }

    private sealed class TickRunnable(MainActivity activity) : Java.Lang.Object, Java.Lang.IRunnable
    {
        public void Run()
        {
            activity._adapter?.RefreshDisplay();
            if (activity._tickRunning)
                activity._handler.PostDelayed(this, 500);
        }
    }

#pragma warning disable CA1422 // OnActivityResult still used for broad API coverage
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode != RequestScan || resultCode != Result.Ok || data is null)
            return;

        string? raw = data.GetStringExtra(ScanActivity.ExtraOtpAuthUri);
        if (string.IsNullOrEmpty(raw))
            return;

        if (!OtpAuthParser.TryParse(raw, out OtpAuthEntry? e, out string? err) || e is null)
        {
            Toast.MakeText(this, GetString(Resource.String.error_parse, OtpAuthParser.DescribeParseError(err)), ToastLength.Long)?.Show();
            return;
        }

        _store?.Add(new AccountEntry
        {
            Issuer = e.Issuer,
            AccountName = e.AccountName,
            SecretBase32 = e.SecretBase32,
            Digits = e.Digits,
            PeriodSeconds = e.PeriodSeconds
        });
        _adapter?.Reload();
    }
#pragma warning restore CA1422
}
