﻿// <copyright file="SolutionEntityAligner.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

namespace WARP.XrmSolutionAssistant.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using NLog;

    /// <summary>
    /// A class to align the entity type codes into a consistent format.
    /// </summary>
    public class SolutionEntityAligner
    {
        /// <summary>
        /// Stores the name of the entity XML file.
        /// </summary>
        private const string EntityFileName = "Entity.xml";

        /// <summary>
        /// NLog reference for this class.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string solutionRootDirectory;

        /// <summary>The fixed Object Type Codes for CRM.</summary>
        private readonly Dictionary<string, int> fixedObjTypeCodes;

        /// <summary>
        /// Initialises a new instance of the <see cref="SolutionEntityAligner"/> class.
        /// </summary>
        /// <param name="solutionRootDirectory">Path to the directory containing the extracted solution.</param>
        public SolutionEntityAligner(string solutionRootDirectory)
        {
            this.solutionRootDirectory = solutionRootDirectory;
            this.fixedObjTypeCodes = GetObjectTypeCodes();
        }

        /// <summary>
        /// Executes the logic to align entity type codes.
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
                var entitiesDir = Path.Combine(this.solutionRootDirectory, "Entities");

                Logger.Debug("Checking directory {0}...", entitiesDir);
                foreach (var entityDir in Directory.GetDirectories(entitiesDir))
                {
                    var xmlPath = Path.Combine(entityDir, EntityFileName);

                    if (!File.Exists(xmlPath))
                    {
                        // Empty directory left over from removed entity, skip
                        Logger.Trace("Empty directory. Skipping: {0}", entitiesDir);
                        continue;
                    }

                    var entityName = entityDir.Substring(entityDir.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                    Logger.Trace("Working on entity: {0}", entityName);

                    var xmlContents = File.ReadAllText(xmlPath);

                    if (this.fixedObjTypeCodes.ContainsKey(entityName))
                    {
                        var newTypeCode = this.fixedObjTypeCodes[entityName];
                        Logger.Trace("Found OTC {0} for entity {1}.", newTypeCode, entityName);

                        var typeCodeRegex = Regex.Match(xmlContents, @"<ObjectTypeCode>(\d+)</ObjectTypeCode>");

                        if (!typeCodeRegex.Success || typeCodeRegex.Groups.Count < 2)
                        {
                            // Couldn't find the correct pattern
                            Logger.Warn("Couldn't find <ObjectTypeCode> tag for entity {0}.", entityName);
                            continue;
                        }

                        var currentTypeCode = typeCodeRegex.Groups[1].ToString();
                        Logger.Trace("Current OTC for entity {0}: {1}", entityName, currentTypeCode);

                        if (string.Compare(currentTypeCode, newTypeCode.ToString(), StringComparison.Ordinal) == 0)
                        {
                            // No change, move to next entity
                            Logger.Trace("New and existing OTC match for entity {0}.", entityName);
                            continue;
                        }

                        Logger.Trace("Object code changed to {0}, reseting entity {1}.", newTypeCode, entityName);

                        var replaceMasks = new[] { "<ObjectTypeCode>{0}</ObjectTypeCode>", "<PrimaryEntityTypeCode>{0}</PrimaryEntityTypeCode>" };

                        xmlContents = replaceMasks.Aggregate(xmlContents, (current, mask) => current.Replace(string.Format(mask, currentTypeCode), string.Format(mask, newTypeCode)));

                        UpdateSavedQueryFilesObjectCode(Path.Combine(entityDir, "SavedQueries"), currentTypeCode, newTypeCode.ToString());
                    }
                }

                Logger.Info("Finished.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unexpected error: {0}", ex.Message);
            }
            finally
            {
                Logger.Info("Leaving {0}", Logger.Name);
            }
        }

        private static Dictionary<string, int> GetObjectTypeCodes()
        {
            Logger.Debug("Getting OTC list.");
            var settings = Workers.Settings.GetSettings();
            var typeCodes = settings.EntityTypeCodes;

            return typeCodes.ToDictionary(tc => tc.EntityLogicalName, tc => tc.TypeCode);
        }

        /// <summary>The update saved query files object code.</summary>
        /// <param name="path">The path to the saved queries.</param>
        /// <param name="oldCode">The old object code.</param>
        /// <param name="newCode">The new object code.</param>
        private static void UpdateSavedQueryFilesObjectCode(string path, string oldCode, string newCode)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                var fileContent = File.ReadAllText(file);
                var replaceMasks = new[] { "object=\"{0}\"", "<returnedtypecode>{0}</returnedtypecode>" };

                foreach (var mask in replaceMasks)
                {
                    fileContent = fileContent.Replace(string.Format(mask, oldCode), string.Format(mask, newCode));
                }

                var tw = new StreamWriter(file, false, new UTF8Encoding(true));
                tw.Write(fileContent);
                tw.Close();
            }
        }
    }
}