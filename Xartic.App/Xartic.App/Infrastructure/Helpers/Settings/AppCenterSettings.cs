using Newtonsoft.Json;
using Xamarin.Forms.Internals;

namespace Xartic.App.Infrastructure.Helpers
{
    [JsonObject("appcenter")]
    public sealed class AppCenterSettings
    {
        [Preserve]
        public AppCenterSettings()
        {
        }

        [JsonProperty("android")]
        public string AndroidKey { get; set; }

        [JsonProperty("ios")]
        public string IosKey { get; set; }
    }
}
