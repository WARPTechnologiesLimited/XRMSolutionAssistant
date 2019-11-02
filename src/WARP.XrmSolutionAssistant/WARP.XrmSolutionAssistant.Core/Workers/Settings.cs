// <copyright file="Settings.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

namespace WARP.XrmSolutionAssistant.Core.Workers
{
    using System.IO;
    using System.Reflection;

    using Newtonsoft.Json;

    using WARP.XrmSolutionAssistant.Core.Models;

    /// <summary>
    /// Class for working with the settings.json file.
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// Helper to read and return the settings from the settings.json config file.
        /// </summary>
        /// <returns>Deserialised SettingsWrapper object.</returns>
        internal static SettingsWrapper GetSettings()
        {
            const string ConfigFileName = "settings.json";

            // Walk the tree to find the variables files
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var filePath = Path.Combine(path, ConfigFileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Cannot locate settings.json file in the following location: {filePath}");
            }

            var json = File.ReadAllText(filePath);

            var settings = JsonConvert.DeserializeObject<SettingsWrapper>(json);
            return settings;
        }
    }
}
