// <copyright file="SolutionVersionReset.cs" company="WARP Technologies Limited">
// Copyright © WARP Technologies Limited
// </copyright>

namespace WARP.XrmSolutionAssistant
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using NLog;

    /// <summary>
    /// Class to reset the Version of a solution to 0.0.0.0 to reduce source control noise.
    /// </summary>
    public class SolutionVersionReset
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string solutionRootDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionVersionReset"/> class.
        /// </summary>
        /// <param name="solutionRootDirectory">Path to the directory containing the extracted solution.</param>
        public SolutionVersionReset(string solutionRootDirectory)
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
                var otherDir = Path.Combine(this.solutionRootDirectory, "Other");

                var solutionXmlPath = Path.Combine(otherDir, "solution.xml");
                var solutionXmlContents = File.ReadAllText(solutionXmlPath);

                // Reset Introduced version number to 0 to prevent changes being marked
                solutionXmlContents = Regex.Replace(solutionXmlContents, @"<Version>\d+.\d+.\d+.\d+</Version>", "<Version>0.0.0.0</Version>");

                // Write the updated solution file
                var sw = new StreamWriter(solutionXmlPath, false, new UTF8Encoding(true));
                sw.Write(solutionXmlContents);
                sw.Close();

                Logger.Info("Reset Solution Version to 0.0.0.0");
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
    }
}
