// <copyright file="Sorter.cs" company="WARP Technologies Limited">
// Copyright © WARP Technologies Limited
// </copyright>

// ReSharper disable PossibleMultipleEnumeration
namespace WARP.XrmSolutionAssistant.Workers
{
    using System.Linq;
    using System.Xml.Linq;

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
        /// </summary>
        /// <param name="doc">The document to sort.</param>
        private static void SortMissingDependencies(XContainer doc)
        {
            const string MissingDependencyLabel = "MissingDependency";
            const string RequiredName = "Required";
            const string DependentName = "Dependent";
            const string KeyName = "key";

            var md = doc.Descendants(MissingDependencyLabel);
            if (!md.Any())
            {
                return;
            }

            var missingDependenciesContainer = md.FirstOrDefault()?.Parent;
            var sorted = md.OrderBy(r => r.Element(RequiredName)?.Attribute(KeyName)?.Value)
                .ThenBy(d => d.Element(DependentName)?.Attribute(KeyName)?.Value).ToList();
            md.Remove();
            foreach (var dependencyElement in sorted)
            {
                missingDependenciesContainer?.Add(dependencyElement);
            }
        }
    }
}