// <copyright file="Sorter.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

// ReSharper disable PossibleMultipleEnumeration
namespace WARP.XrmSolutionAssistant.Core.Workers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using WARP.XrmSolutionAssistant.Core.Comparers;

    /// <summary>
    /// Class for managing the sorting of an XML Document.
    /// </summary>
    internal class Sorter
    {
        /// <summary>
        /// Executes the sorting of the document.
        /// </summary>
        /// <param name="doc">The xml document to sort.</param>
        /// <param name="docHasChanged">Flag indicating whether any changes have been made to the document.</param>
        public void Sort(XDocument doc, out bool docHasChanged)
        {
            var unsortedDoc = new XDocument(doc);

            SortContainerByAttributeValue(doc, "labels", "languagecode");
            SortContainerByAttributeValue(doc, "displaynames", "languagecode");
            SortContainerByAttributeValue(doc, "Descriptions", "languagecode"); // Entity.xml and others
            SortContainerByAttributeValue(doc, "LocalizedNames", "languagecode"); // Entity.xml, Solution.xml
            SortContainerByAttributeValue(doc, "LocalizedCollectionNames", "languagecode"); // Entity.xml
            SortContainerByAttributeValue(doc, "Descriptions", "LCID"); // Sitemap
            SortContainerByAttributeValue(doc, "Titles", "LCID"); // Sitemap
            SortContainerByAttributeValue(doc, "CustomLabels", "languagecode"); // Relationships
            SortContainerByAttributeValue(doc, "AppModuleRoleMaps", "id"); // AppModule.xml

            SortMissingDependencies(doc);

            docHasChanged = !XNode.DeepEquals(unsortedDoc, doc);
        }

        private static void SortContainerByAttributeValue(XContainer doc, string containerName, string attributeToSort)
        {
            var xml = doc.Descendants(containerName);

            xml.ToList().ForEach(
                x =>
                    {
                        // Only sort elements that have the required attribute
                        var elementsToSort = x.Elements().Where(el => el.Attribute(attributeToSort) != null);
                        var sorted = elementsToSort.OrderBy(s => s.Attribute(attributeToSort)?.Value).ToList();
                        elementsToSort.Remove();
                        foreach (var element in sorted)
                        {
                            x.Add(element);
                        }
                    });
        }

        /// <summary>
        /// Sorts the MissingDependencies block by Required key then Dependent key.
        /// This is achieved by ordering all the dependencies in a predictable way then reassign keys based on this order.
        /// </summary>
        /// <param name="doc">The document to sort.</param>
        private static void SortMissingDependencies(XContainer doc)
        {
            const string MissingDependencyLabel = "MissingDependency";
            const string RequiredName = "Required";
            const string DependentName = "Dependent";

            const string AttributeKey = "key";
            const string AttributeType = "type";
            const string AttributeSchemaName = "schemaName";
            const string AttributeDisplayName = "displayName";
            const string AttributeParentSchemaName = "parentSchemaName";
            const string AttributeParentDisplayName = "parentDisplayName";
            const string AttributeId = "id";
            const string AttributeSolutionId = "solutionId";
            const string AttributeNewKey = "assistant_newKey";

            var md = doc.Descendants(MissingDependencyLabel);
            if (!md.Any())
            {
                return;
            }

            var missingDependenciesContainer = md.FirstOrDefault()?.Parent;

            var dependencyList = new Dictionary<int, Dictionary<string, string>>();

            foreach (var dependency in md)
            {
                foreach (var line in dependency.Elements())
                {
                    if (int.TryParse(line.Attribute(AttributeKey)?.Value, out var key) == false)
                    {
                        // MissingDependenies are of the format that does not contain keys.
                        return;
                    }

                    // Add each dependency to the list and create the key value pairs for the expected attributes. These will be populated as lines are found ensuring the elements exist for all items when sorting.
                    if (!dependencyList.ContainsKey(key))
                    {
                        dependencyList.Add(key, new Dictionary<string, string> { { AttributeKey, null }, { AttributeType, null }, { AttributeSchemaName, null }, { AttributeDisplayName, null }, { AttributeParentSchemaName, null }, { AttributeParentDisplayName, null }, { AttributeId, null }, { AttributeSolutionId, null }, { AttributeNewKey, null } });
                    }

                    // Go through each line and fill the dependency list and attributes
                    foreach (var xAttribute in line.Attributes())
                    {
                        var kvp = new KeyValuePair<string, string>(xAttribute.Name.LocalName, xAttribute.Value);
                        if (dependencyList[key].ContainsKey(kvp.Key))
                        {
                            dependencyList[key][kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            // Sort the dependencies now we have all the required information
            var sortedDependencies = dependencyList.OrderBy(d => d.Value[AttributeSchemaName])
                .ThenBy(d => d.Value[AttributeDisplayName])
                .ThenBy(d => d.Value[AttributeSolutionId])
                .ThenBy(d => d.Value[AttributeId])
                .ThenBy(d => d.Value[AttributeParentSchemaName])
                .ThenBy(d => d.Value[AttributeParentDisplayName])
                .ThenBy(d => d.Value[AttributeType]).ToList();

            // Add the new keys for each dependency
            var count = 1;
            foreach (var keyValuePair in sortedDependencies)
            {
                int.TryParse(keyValuePair.Value[AttributeKey], out var key);
                dependencyList[key][AttributeNewKey] = count.ToString();
                count++;
            }

            // Update the keys in the XML
            foreach (var dependency in md)
            {
                foreach (var line in dependency.Elements())
                {
                    if (line.Attribute(AttributeKey)?.Value != null && int.TryParse(line.Attribute(AttributeKey).Value, out var key))
                    {
                        line.Attribute(AttributeKey).Value = dependencyList[key][AttributeNewKey];
                    }
                }
            }

            // Order the XML by the new keys
            var comparer = new StringNumericCompare();
            var sorted = md.OrderBy(r => r.Element(RequiredName)?.Attribute(AttributeKey)?.Value, comparer).ThenBy(d => d.Element(DependentName)?.Attribute(AttributeKey)?.Value, comparer).ToList();

            md.Remove();
            foreach (var dependencyElement in sorted)
            {
                missingDependenciesContainer?.Add(dependencyElement);
            }
        }
    }
}
