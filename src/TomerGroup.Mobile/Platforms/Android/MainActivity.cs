#if ANDROID
using Android.App;
using Android.Content.PM;
using Android.OS;

namespace TomerGroup.Mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, Icon = "@mipmap/appicon", RoundIcon = "@mipmap/appicon_round", LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
#endif

