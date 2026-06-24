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
    private Action? _tick;

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
        var container = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };
        int pad = (int)(16 * Resources!.DisplayMetrics!.Density);
        container.SetPadding(pad, pad, pad, pad);

        var issuer = new EditText(this) { Hint = GetString(Resource.String.hint_issuer) };
        var secret = new EditText(this) { Hint = GetString(Resource.String.hint_secret) };
        secret.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationVisiblePassword;
        container.AddView(issuer, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        container.AddView(secret, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

        new AndroidX.AppCompat.App.AlertDialog.Builder(this!)
            .SetTitle(Resource.String.manual_title)
            .SetView(container)
            .SetNegativeButton(Resource.String.btn_cancel, (s, e) => { })
            .SetPositiveButton(Resource.String.btn_ok, (s, e) =>
            {
                string iss = issuer.Text?.ToString()?.Trim() ?? "";
                string sec = secret.Text?.ToString()?.Trim() ?? "";
                TryAddFromManual(iss, sec);
                HideKeyboard(issuer);
            })
            .Show();
    }

    private void TryAddFromManual(string issuer, string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            Toast.MakeText(this, GetString(Resource.String.error_secret_empty), ToastLength.Short)?.Show();
            return;
        }

        if (!SecretBytes.TryDecodeSecret(secret, out _, out string? err))
        {
            Toast.MakeText(this, SecretBytes.DescribeDecodeError(err), ToastLength.Short)?.Show();
            return;
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
    }

    private void HideKeyboard(Android.Views.View view)
    {
        var imm = (InputMethodManager?)GetSystemService(Context.InputMethodService!);
        imm?.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
    }

    protected override void OnResume()
    {
        base.OnResume();
        _adapter?.Reload();
        _tick = Tick;
        _handler.PostDelayed(_tick, 200);
    }

    private void Tick()
    {
        _adapter?.NotifyDataSetChanged();
        if (_tick != null)
            _handler.PostDelayed(_tick, 500);
    }

    protected override void OnPause()
    {
        base.OnPause();
        if (_tick != null)
            _handler.RemoveCallbacks(_tick);
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
            Toast.MakeText(this, string.Format(GetString(Resource.String.error_parse), err ?? ""), ToastLength.Long)?.Show();
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
