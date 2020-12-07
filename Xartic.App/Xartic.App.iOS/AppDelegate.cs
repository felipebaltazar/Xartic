using FFImageLoading.Forms.Platform;
using FFImageLoading.Svg.Forms;
using Foundation;
using UIKit;
using Xartic.App.Abstractions;

namespace Xartic.App.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IPlatformInitializer
    {
        protected Startup Initializer { get; private set; }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            CachedImageRenderer.Init();
            var ignore = typeof(SvgCachedImage);

            Initializer = new Startup(this);
            LoadApplication(Initializer.ResolveApplication());

            return base.FinishedLaunching(app, options);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Register platform specifc services
        }
    }
}
