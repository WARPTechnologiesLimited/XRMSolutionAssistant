// <copyright file="AssemblyAndStepVersionReset.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

namespace WARP.XrmSolutionAssistant.Core
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using NLog;

    /// <summary>
    /// Class to reset the Version of a solution assembly and step components to the value defined in settings.json.
    /// </summary>
    public class AssemblyAndStepVersionReset
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string solutionRootDirectory;

        /// <summary>
        /// Initialises a new instance of the <see cref="AssemblyAndStepVersionReset"/> class.
        /// </summary>
        /// <param name="solutionRootDirectory">Path to the directory containing the extracted solution.</param>
        public AssemblyAndStepVersionReset(string solutionRootDirectory)
        {
            this.solutionRootDirectory = solutionRootDirectory;
        }

        /// <summary>
        /// Gets the version to reset the components to.
        /// </summary>
        public string ResetVersion { get; private set; }

        /// <summary>
        /// Executes the logic to reset the Version of a solution assembly and step components to the value defined in settings.json to reduce source control noise.
        /// </summary>
        public void Execute()
        {
            Logger.Info("Solution Root Directory: {0}", this.solutionRootDirectory);

            if (!Directory.Exists(this.solutionRootDirectory))
            {
                Logger.Fatal("The given solution root directory does not exist or is not available. Exiting.");
                return;
            }

            try
            {
                var settings = Workers.Settings.GetSettings();
                this.ResetVersion = settings.AssemblyAndStepVersionReset.ResetVersion;
                Logger.Info("Reseting Assembly Versions to {0}", this.ResetVersion);

                // traverse the root directory.
                ProcessDirectory(this.solutionRootDirectory, this.ResetVersion);

                Logger.Info("Reset Assembly Versions to {0}", this.ResetVersion);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Unexpected error: {0}", ex.Message);
            }
            finally
            {
                Logger.Info("Leaving {0}", Logger.Name);
            }
        }

        private static void ProcessDirectory(string targetDirectory, string version)
        {
            // Process the list of files found in the directory.
            var fileEntries = Directory.GetFiles(targetDirectory, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (var fileName in fileEntries)
            {
                ProcessFile(fileName, version);
            }

            // Recurse into subdirectories of this directory.
            var subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (var subdirectory in subdirectoryEntries)
            {
                ProcessDirectory(subdirectory, version);
            }
        }

        private static void ProcessFile(string path, string version)
        {
            var xmlContents = File.ReadAllText(path);
            var reg = new Regex(@", Version=.+\d,");
            if (reg.IsMatch(xmlContents))
            {
                // Reset Assembly version number to version to prevent changes being marked.
                xmlContents = reg.Replace(xmlContents, $", Version={version},");
                var ew = new StreamWriter(path, false, new UTF8Encoding(true));
                ew.Write(xmlContents);
                ew.Close();
            }
        }
    }
}
