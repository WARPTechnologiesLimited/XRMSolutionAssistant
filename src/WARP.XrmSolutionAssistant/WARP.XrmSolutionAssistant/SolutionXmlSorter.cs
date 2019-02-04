// -----------------------------------------------------------------------
// <copyright file="SolutionXmlSorter.cs" company="WARP Technologies Limited">
// Copyright © WARP Technologies Limited
// </copyright>
// -----------------------------------------------------------------------

namespace WARP.XrmSolutionAssistant
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using NLog;
    using WARP.XrmSolutionAssistant.Workers;

    /// <summary>
    /// Sorts extracted Dynamics CRM solution files.
    /// </summary>
    public class SolutionXmlSorter
    {
        /// <summary>
        /// NLog reference for this class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string solutionRootDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionXmlSorter"/> class.
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
                    filename =>
                        {
                            var doc = XDocument.Load(filename);
                            if (doc.Root != null && doc.Root.Name == "Workflow")
                            {
                                Logger.Info("Skipping workflow: {0}", filename);
                            }
                            else
                            {
                                sorter.Sort(doc, out var hasChanged);
                                if (hasChanged)
                                {
                                    Logger.Info("File sorted: {0}", filename);
                                    doc.Save(filename);
                                    sorted++;
                                }
                                else
                                {
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
