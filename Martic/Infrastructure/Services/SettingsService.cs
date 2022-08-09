using Martic.Abstractions.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Xartic.Core.Extensions;

namespace Martic.Infrastructure.Services
{
    public sealed class SettingsService : ISettingsService
    {
        #region Fields

        private static readonly JsonSerializer _serializer = new JsonSerializer();

        private readonly Lazy<JObject> _settingsFile;
        private readonly JsonMergeSettings _mergeOptions;

        #endregion

        #region Constructors

        public SettingsService()
        {
            _settingsFile = new Lazy<JObject>(() => LoadSettingsFile());
            _mergeOptions = new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            };
        }

        #endregion

        #region ISettingsService

        /// <inheritdoc/>
        public TResult GetValue<TResult>()
        {
            var resultType = typeof(TResult);
            var jObjectAtt =
            (JsonObjectAttribute)Attribute.GetCustomAttribute(resultType, typeof(JsonObjectAttribute));

            var objectName = jObjectAtt?.Id ?? resultType.Name;
            var jValue = GetValue(objectName);

            return JsonConvert.DeserializeObject<TResult>(jValue);
        }

        /// <inheritdoc/>
        public string GetValue(string key)
        {
            var jSettings = _settingsFile.Value;
            var jValue = jSettings.GetValue(key, StringComparison.OrdinalIgnoreCase);
            return jValue.ToString();
        }

        #endregion

        #region Private Methods

        private JObject LoadSettingsFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var prodRes = resources.GetResource("settings.json");
            var jProd = LoadSettingsJObject(prodRes, assembly);


            return jProd;
        }

        private static JObject LoadSettingsJObject(string fileName, Assembly assembly)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new InvalidOperationException($"{fileName} file not found");

            using (var file = assembly.GetManifestResourceStream(fileName))
            {
                using (var streamReader = new StreamReader(file))
                {
                    using (var textReader = new JsonTextReader(streamReader))
                    {
                        return _serializer.Deserialize<JObject>(textReader);
                    }
                }
            }
        }

        #endregion
    }
}
