using Newtonsoft.Json;

namespace Martic.Infrastructure.Helpers.Settings
{
    [JsonObject("appcenter")]
    public sealed class AppCenterSettings
    {
        public AppCenterSettings()
        {
        }

        [JsonProperty("android")]
        public string AndroidKey { get; set; }

        [JsonProperty("ios")]
        public string IosKey { get; set; }
    }
}
