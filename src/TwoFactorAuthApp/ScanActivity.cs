using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.Util.Concurrent;
using TwoFactorAuthApp.Camera;

namespace TwoFactorAuthApp;

[Activity(Theme = "@style/AppTheme")]
public class ScanActivity : AppCompatActivity
{
    public const string ExtraOtpAuthUri = "otpauth_uri";
    private PreviewView? _preview;
    private ProcessCameraProvider? _cameraProvider;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_scan);
        _preview = FindViewById<PreviewView>(Resource.Id.previewView)!;
        FindViewById<Android.Views.View>(Resource.Id.btnClose)!.Click += (_, _) => Finish();

        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted)
            StartCamera();
        else
            ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.Camera }, 1);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == 1 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            StartCamera();
    }

    private void StartCamera()
    {
        if (_preview is null)
            return;

        var future = ProcessCameraProvider.GetInstance(this);
        future.AddListener(new CameraOpenListener(this), ContextCompat.GetMainExecutor(this)!);
    }

    private void BindCamera(PreviewView previewView)
    {
        if (_cameraProvider is null)
            return;

        _cameraProvider.UnbindAll();

        var preview = new Preview.Builder().Build();
        preview.SetSurfaceProvider(ContextCompat.GetMainExecutor(this)!, previewView.SurfaceProvider!);

        var analysisBuilder = new ImageAnalysis.Builder()
            .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest);
        analysisBuilder.SetOutputImageFormat(2);

        ImageAnalysis analysis = analysisBuilder.Build();
        var analyzer = new QrFrameAnalyzer(uri =>
        {
            var data = new Intent();
            data.PutExtra(ExtraOtpAuthUri, uri);
            SetResult(Android.App.Result.Ok, data);
            Finish();
        });
        analysis.SetAnalyzer(Executors.NewSingleThreadExecutor()!, analyzer);

        _cameraProvider.BindToLifecycle(this, CameraSelector.DefaultBackCamera!, preview, analysis);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _cameraProvider?.UnbindAll();
    }

    private sealed class CameraOpenListener(ScanActivity activity) : Java.Lang.Object, Java.Lang.IRunnable
    {
        public void Run()
        {
            try
            {
                var f = ProcessCameraProvider.GetInstance(activity);
                activity._cameraProvider = (ProcessCameraProvider)f.Get()!;
                if (activity._preview is not null)
                    activity.BindCamera(activity._preview);
            }
            catch (Exception)
            {
                activity.RunOnUiThread(activity.Finish);
            }
        }
    }
}
