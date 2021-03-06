﻿// -----------------------------------------------------------------------
// <copyright file="SolutionXmlSorter.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>
// -----------------------------------------------------------------------

namespace WARP.XrmSolutionAssistant.Core
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using NLog;
    using WARP.XrmSolutionAssistant.Core.Workers;

    /// <summary>
    /// Sorts extracted Dynamics CRM solution files.
    /// </summary>
    public class SolutionXmlSorter
    {
        /// <summary>
        /// NLog reference for this class.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string solutionRootDirectory;

        /// <summary>
        /// Initialises a new instance of the <see cref="SolutionXmlSorter"/> class.
        /// </summary>
        /// <param name="solutionRootDirectory">The root directory for the extracted solution.</param>
        public SolutionXmlSorter(string solutionRootDirectory)
        {
            this.solutionRootDirectory = solutionRootDirectory;
            Logger.Debug("Created instance of {0}", nameof(SolutionXmlSorter));
        }

        /// <summary>
        /// Executes sorting logic.
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
                var sorter = new Sorter();
                var sorted = 0;
                var alreadySorted = 0;
                var files = Directory.EnumerateFiles(this.solutionRootDirectory, "*.xml", SearchOption.AllDirectories);
                files.ToList().ForEach(
                    filePath =>
                        {
                            var fileName = Path.GetFileName(filePath);
                            Logger.Trace("Processing file: {0}", filePath);
                            var doc = XDocument.Load(filePath);
                            if (doc.Root != null && doc.Root.Name == "Workflow")
                            {
                                Logger.Info("Skipping workflow: {0}", fileName);
                            }
                            else
                            {
                                sorter.Sort(doc, out var hasChanged);
                                if (hasChanged)
                                {
                                    Logger.Info("File sorted: {0}", fileName);
                                    doc.Save(filePath);
                                    sorted++;
                                }
                                else
                                {
                                    Logger.Trace("Already sorted. No action: {0}", fileName);
                                    alreadySorted++;
                                }
                            }
                        });

                Logger.Info("{0} files sorted. {1} files didn't need sorting.", sorted, alreadySorted);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in {0}.", nameof(SolutionXmlSorter));
                throw;
            }
            finally
            {
                Logger.Info("Leaving {0}", Logger.Name);
            }
        }
    }
}
