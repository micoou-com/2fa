using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.Util.Concurrent;
using TwoFactorAuthApp.Camera;

namespace TwoFactorAuthApp;

[Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
public class ScanActivity : AppCompatActivity
{
    public const string ExtraOtpAuthUri = "otpauth_uri";
    private PreviewView? _preview;
    private ProcessCameraProvider? _cameraProvider;
    private QrFrameAnalyzer? _analyzer;
    private readonly LatestFrameBuffer _latestFrame = new();
    private bool _decoding;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_scan);
        _preview = FindViewById<PreviewView>(Resource.Id.previewView)!;

        FindViewById<Android.Views.View>(Resource.Id.btnClose)!.Click += (_, _) => Finish();
        FindViewById<Android.Views.View>(Resource.Id.btnCapture)!.Click += (_, _) => CaptureStill();

        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted)
            StartCamera();
        else
            ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.Camera }, 1);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode != 1)
            return;

        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            StartCamera();
        else
        {
            Toast.MakeText(this, Resource.String.scan_camera_denied, ToastLength.Long)?.Show();
            Finish();
        }
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
            .SetBackpressureStrategy(ImageAnalysis.StrategyKeepOnlyLatest)
            .SetOutputImageFormat(ImageAnalysis.OutputImageFormatYuv420888);

        ImageAnalysis analysis = analysisBuilder.Build();
        _analyzer = new QrFrameAnalyzer(CompleteWithUri, _latestFrame);
        analysis.SetAnalyzer(Executors.NewSingleThreadExecutor()!, _analyzer);

        _cameraProvider.BindToLifecycle(this, CameraSelector.DefaultBackCamera!, preview, analysis);
    }

    private void CaptureStill()
    {
        if (_decoding)
            return;

        if (_analyzer is null)
        {
            Toast.MakeText(this, Resource.String.scan_camera_error, ToastLength.Short)?.Show();
            return;
        }

        _decoding = true;
        var captureBtn = FindViewById<Android.Views.View>(Resource.Id.btnCapture)!;
        captureBtn.Enabled = false;

        Task.Run(() =>
        {
            _analyzer.TryDecodeLatest(
                uri => RunOnUiThread(() =>
                {
                    _decoding = false;
                    CompleteWithUri(uri);
                }),
                () => RunOnUiThread(() =>
                {
                    _decoding = false;
                    captureBtn.Enabled = true;
                    Toast.MakeText(this, Resource.String.scan_no_qr, ToastLength.Long)?.Show();
                }));
        });
    }

    private void CompleteWithUri(string uri)
    {
        var data = new Intent();
        data.PutExtra(ExtraOtpAuthUri, uri);
        SetResult(Android.App.Result.Ok, data);
        Finish();
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
                activity.RunOnUiThread(() =>
                {
                    Toast.MakeText(activity, Resource.String.scan_camera_error, ToastLength.Long)?.Show();
                    activity.Finish();
                });
            }
        }
    }
}
