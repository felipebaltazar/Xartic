using Android.App;
using Android.Content.PM;
using Android.OS;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Svg.Forms;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xartic.App.Abstractions;

namespace Xartic.App.Droid
{
    [Activity(Label = "Xartic", Icon = "@mipmap/icon", NoHistory = true, Theme = "@style/SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public sealed class SplashActivity : Activity, IPlatformInitializer
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartMainActivityAsync(savedInstanceState);
        }

        private void StartMainActivityAsync(Bundle savedInstanceState)
        {
            _ = Task.Run(() =>
            {
                Platform.Init(this, savedInstanceState);
                CachedImageRenderer.Init(true);
                Forms.Init(this, savedInstanceState);

                _ = typeof(SvgCachedImage);
                _ = new Startup(this);

                StartActivity(typeof(MainActivity));
                Finish();
            });
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Register platform specifc services
        }
    }
}