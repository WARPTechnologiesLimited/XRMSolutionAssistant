// <copyright file="SolutionFlowConnectionMapper.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

namespace WARP.XrmSolutionAssistant.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using NLog;
    using WARP.XrmSolutionAssistant.Core.Models;

    /// <summary>
    /// Class to map the Flow connections to those known by a target.
    /// </summary>
    public class SolutionFlowConnectionMapper
    {
        private const string ConnectionReferences = "connectionreferences";
        private const string ConnectionReference = "connectionreference";
        private const string ConnectionReferenceLogicalName = "connectionreferencelogicalname";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string solutionRootDirectory;

        private readonly Dictionary<string, string> replacements;

        private readonly List<string> unmappedApis;

        private readonly List<string> customizationConnectionsToDelete;

        private SettingsWrapper settings;

        /// <summary>
        /// Initialises a new instance of the <see cref="SolutionFlowConnectionMapper"/> class.
        /// </summary>
        /// <param name="solutionRootDirectory">Path to the directory containing the extracted solution.</param>
        public SolutionFlowConnectionMapper(string solutionRootDirectory)
        {
            this.solutionRootDirectory = solutionRootDirectory;
            this.replacements = new Dictionary<string, string>();
            this.unmappedApis = new List<string>();
            this.customizationConnectionsToDelete = new List<string>();
            this.settings = Workers.Settings.GetSettings();
        }

        /// <summary>
        /// Executes the logic to map the Flow connection references to the values defined in settings.json.
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
                var customizationsXmlPath = Path.Combine(otherDir, "customizations.xml");

                Logger.Info("Getting connection references from customizations.xml.");
                var customizationsConnReferences = this.GetConnectionReferencesFromCustomizations(customizationsXmlPath);
                Logger.Info($"{customizationsConnReferences.Count} connection references retrieved from customizations.xml.");

                this.ParseCustomizationReferences(customizationsConnReferences);
                if (this.unmappedApis.Any())
                {
                    var unmappedApisString = $"The following Flow apis do not have mappings; {string.Join(",", this.unmappedApis.Distinct())}";
                    if (this.settings.FlowConnectionMapper.ThrowExceptionOnUnmappedConnections)
                    {
                        Logger.Error(unmappedApisString);
                        throw new Exception(unmappedApisString);
                    }

                    Logger.Warn(unmappedApisString);
                }

                this.ProcessCustomizationsFile(customizationsXmlPath);
                this.ProcessFlowJsons();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "Unexpected error: {0}", ex);
                throw ex;
            }
            finally
            {
                Logger.Info("Leaving {0}", Logger.Name);
            }
        }

        /// <summary>
        /// Gets a list of connection references from the customizations file.
        /// </summary>
        /// <param name="customizationsXmlPath">Path to the customizations.xml file.</param>
        /// <returns>A List of CustomizationConnectionReferences.</returns>
        private List<CustomizationsConnectionReference> GetConnectionReferencesFromCustomizations(string customizationsXmlPath)
        {
            // Get and read customizations.xml.
            var customizationsXmlContents = XElement.Load(customizationsXmlPath);

            return this.GetConnectionReferencesFromCustomizations(customizationsXmlContents);
        }

        /// <summary>
        /// Gets a list of connection references from the customizations XElement.
        /// </summary>
        /// <param name="customizationsXmlContents">XElement of the customizations.xml file.</param>
        /// <returns>A List of CustomizationConnectionReferences.</returns>
        private List<CustomizationsConnectionReference> GetConnectionReferencesFromCustomizations(XElement customizationsXmlContents)
        {
            // Get the connection references.
            var connectionReferencesElement = customizationsXmlContents.Element(ConnectionReferences);
            var connectionReferenceElements = from el in connectionReferencesElement.Elements(ConnectionReference) select el;

            var connectionReferences = new List<CustomizationsConnectionReference>();

            foreach (var el in connectionReferenceElements)
            {
                // Deaerialize each connectionreference element into a class.
                var ser = new XmlSerializer(typeof(CustomizationsConnectionReference));
                var conref = (CustomizationsConnectionReference)ser.Deserialize(el.CreateReader());
                if (conref != null)
                {
                    if (connectionReferences.Any(x => x.ApiName == conref.ApiName))
                    {
                        // We have already got a connection for this Api. Tag this connection for removal.
                        Logger.Warn($"Removing connection name [{conref.ConnectionReferenceLogicalName}] from customizations as it is a duplicate for api [{conref.ApiName}].");
                        this.customizationConnectionsToDelete.Add(conref.ConnectionReferenceLogicalName);
                        continue;
                    }

                    connectionReferences.Add(conref);
                }
            }

            return connectionReferences;
        }

        private void ProcessFlowJsons()
        {
            Logger.Info("Processing json files.");
            var workflowsDirectory = Path.Combine(this.solutionRootDirectory, "Workflows");
            var flowJsonFiles = Directory.GetFiles(workflowsDirectory, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var jsonFile in flowJsonFiles)
            {
                Logger.Trace($"Reading {jsonFile}");
                var jsonContents = File.ReadAllText(jsonFile);
                foreach (var rep in this.replacements)
                {
                    Logger.Trace($"Replacing {rep.Key} with {rep.Value}");
                    jsonContents = jsonContents.Replace(rep.Key, rep.Value);

                    // Write the updated json file.
                    Logger.Info("Saving json file.");
                    var sw = new StreamWriter(jsonFile, false, new UTF8Encoding(true));
                    sw.Write(jsonContents);
                    sw.Close();
                }
            }
        }

        /// <summary>
        /// Replaces the connection references in the customizations with those from the maps.
        /// </summary>
        /// <param name="customizationsXmlPath">Path to the customizations.xml file.</param>
        private void ProcessCustomizationsFile(string customizationsXmlPath)
        {
            Logger.Info("Processing customizations.xml file.");
            var customizationsXml = XElement.Load(customizationsXmlPath);

            if (this.customizationConnectionsToDelete.Count > 0)
            {
                // Delete redundant connections.
                Logger.Info($"Processing deletes from customizations; {string.Join(",", this.customizationConnectionsToDelete)}");
                var elementsToDelete = new List<XElement>();
                this.customizationConnectionsToDelete.ForEach(connectionName =>
                {
                    var elementsWithThisName = from el in customizationsXml.Element(ConnectionReferences).Elements(ConnectionReference)
                                  where (string)el.Attribute(ConnectionReferenceLogicalName) == connectionName
                                  select el;

                    Logger.Trace($"Adding {connectionName} elements to list of elements to delete.");
                    elementsToDelete.AddRange(elementsWithThisName);
                });

                if (elementsToDelete.Any())
                {
                    elementsToDelete.ForEach(el => el.Remove());
                }
            }

            // Loop through the replacements dictionary, renaming connections.
            foreach (var rep in this.replacements)
            {
                Logger.Trace($"Replacing {rep.Key} with {rep.Value}");
                var elsToRename = (from el in customizationsXml.Element(ConnectionReferences).Elements(ConnectionReference)
                                  where (string)el.Attribute(ConnectionReferenceLogicalName) == rep.Key
                                  select el).ToList();
                elsToRename.ForEach(el => el.SetAttributeValue(ConnectionReferenceLogicalName, rep.Value));
            }

            Logger.Info("Saving customizations.xml file.");
            customizationsXml.Save(customizationsXmlPath);
        }

        /// <summary>
        /// Processes the customizations connections, building a list of replacement values and a list of unmapped apis.
        /// </summary>
        /// <param name="customizationsConnReferences">List of connection references extracted from the customizations file.</param>
        private void ParseCustomizationReferences(List<CustomizationsConnectionReference> customizationsConnReferences)
        {
            // Get the maps from settings.json.
            var maps = this.settings.FlowConnectionMapper.Maps;

            // Loop through the connections in the customizations.xml file.
            foreach (var customizationConnRef in customizationsConnReferences)
            {
                var apiName = customizationConnRef.ApiName;

                // Get the connection name from the maps for the matching api name.
                var targetConnName = maps.Where(x => x.ApiName == apiName).Select(x => x.ConnectionUniqueName).SingleOrDefault();
                if (string.IsNullOrEmpty(targetConnName))
                {
                    Logger.Warn($"No mapping for api '{apiName}'.");
                    this.unmappedApis.Add(apiName);
                    continue;
                }

                if (customizationConnRef.ConnectionReferenceLogicalName != targetConnName)
                {
                    Logger.Trace($"Adding {targetConnName} will replace {customizationConnRef.ConnectionReferenceLogicalName}");
                    this.replacements.Add(customizationConnRef.ConnectionReferenceLogicalName, targetConnName);
                }
            }
        }
    }
}
