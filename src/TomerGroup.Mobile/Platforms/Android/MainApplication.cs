#if ANDROID
using Android.App;
using Android.Runtime;

namespace TomerGroup.Mobile;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Android.Util.Log.Error("TomerGroup", $"AppDomain UnhandledException: {e.ExceptionObject}");
        };

        AndroidEnvironment.UnhandledExceptionRaiser += (s, e) =>
        {
            Android.Util.Log.Error("TomerGroup", $"AndroidEnvironment UnhandledException: {e.Exception}");
            e.Handled = true; // Prevent app hard crashing
        };

        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Android.Util.Log.Error("TomerGroup", $"TaskScheduler UnobservedTaskException: {e.Exception}");
            e.SetObserved();
        };
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
#endif

