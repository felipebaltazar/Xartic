namespace Xartic.App.Abstractions.Services
{
    public interface ISettingsService
    {
        /// <summary>
        /// Get the settings value from object
        /// </summary>
        /// <typeparam name="TResult">Settings object type to deserialize</typeparam>
        /// <returns>Deserialized setting object</returns>
        TResult GetValue<TResult>();

        /// <summary>
        /// Get the settings value from key
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <returns>Setting string value</returns>
        string GetValue(string key);
    }
}
