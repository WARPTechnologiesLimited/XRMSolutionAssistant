// <copyright file="SolutionWorkflowGuidAligner.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

namespace WARP.XrmSolutionAssistant.Core
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    using NLog;

    /// <summary>
    /// A class to align the Guids within CRM Workflow xml files.
    /// </summary>
    public class SolutionWorkflowGuidAligner
    {
        /// <summary>
        /// NLog reference for this class.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string solutionRootDirectory;

        /// <summary>
        /// Initialises a new instance of the <see cref="SolutionWorkflowGuidAligner"/> class.
        /// </summary>
        /// <param name="solutionRootDirectory">Path to the directory containing the extracted solution.</param>
        public SolutionWorkflowGuidAligner(string solutionRootDirectory)
        {
            this.solutionRootDirectory = solutionRootDirectory;
        }

        /// <summary>
        /// Executes the logic to replace workflow Guids with a consistent value.
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
                var workflowsDir = Path.Combine(this.solutionRootDirectory, "Workflows");

                var xamlPaths = Directory.GetFiles(workflowsDir, "*.xaml");

                foreach (var xamlPath in xamlPaths)
                {
                    var xamlFileName = Path.GetFileNameWithoutExtension(xamlPath);
                    Logger.Info("Working on: {0}", xamlFileName);
                    var xamlContents = File.ReadAllText(xamlPath);
                    const string SearchPattern = "(?<=<Variable x:TypeArguments=\"x:String\" Default=\")([a-z0-9]{8}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{4}-[a-z0-9]{12})";
                    var defaultGuidRegex = Regex.Match(xamlContents, SearchPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

                    if (!defaultGuidRegex.Success)
                    {
                        // Couldn't find the correct pattern
                        Logger.Info("Couldn't find Default Guid tag for Workflow {0}.", xamlFileName);
                        continue;
                    }

                    var oldGuidString = defaultGuidRegex.Groups[1].Value;
                    if (!Guid.TryParse(oldGuidString, out var parsedGuid))
                    {
                        Logger.Warn("Returned Guid string is not a Guid: {0}", oldGuidString);
                        continue;
                    }

                    Logger.Trace("Old Guid: {0}", oldGuidString);
                    var newGuid = StringToGuid(xamlFileName);

                    if (string.Equals(oldGuidString, newGuid.ToString("D"), StringComparison.CurrentCultureIgnoreCase))
                    {
                        Logger.Info("Guids match. Skipping.");
                        continue;
                    }

                    xamlContents = xamlContents.Replace(oldGuidString, newGuid.ToString("D"));

                    var streamWriter = new StreamWriter(xamlPath, false, new UTF8Encoding(true));
                    streamWriter.Write(xamlContents);
                    streamWriter.Close();
                }

                Logger.Info("Finished.");
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

        private static Guid StringToGuid(string value)
        {
            Logger.Trace("String to GUID called for: {0}", value);

            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));

            return new Guid(data);
        }
    }
}
