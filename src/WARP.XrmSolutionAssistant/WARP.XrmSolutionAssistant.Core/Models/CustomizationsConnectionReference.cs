// <copyright file="CustomizationsConnectionReference.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

namespace WARP.XrmSolutionAssistant.Core.Models
{
    using System.Xml.Serialization;

    /// <summary>
    /// Model for a Connection Reference node in the customizations.xml file.
    /// </summary>
    [XmlRoot(ElementName = "connectionreference")]
    public class CustomizationsConnectionReference
    {
        /// <summary>
        /// Gets or sets connectionreferencelogicalname.
        /// </summary>
        [XmlAttribute(AttributeName = "connectionreferencelogicalname")]
        public string ConnectionReferenceLogicalName { get; set; }

        /// <summary>
        /// Gets or sets connectionreferencedisplayname.
        /// </summary>
        [XmlElement(ElementName = "connectionreferencedisplayname")]
        public string ConnectionReferenceDisplayName { get; set; }

        /// <summary>
        /// Gets or sets connectorid.
        /// </summary>
        [XmlElement(ElementName = "connectorid")]
        public string ConnectorId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether iscustomizable.
        /// </summary>
        [XmlElement(ElementName = "iscustomizable")]
        public bool IsCustomizable { get; set; }

        /// <summary>
        /// Gets or sets statecode.
        /// </summary>
        [XmlElement(ElementName = "statecode")]
        public int StateCode { get; set; }

        /// <summary>
        /// Gets or sets statuscode.
        /// </summary>
        [XmlElement(ElementName = "statuscode")]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets the api name from the connectorid.
        /// </summary>
        /// <returns>Name of the api.</returns>
        public string ApiName
        {
            get
            {
                var apiName = string.Empty;
                var lastSlashPos = this.ConnectorId.LastIndexOf('/');
                if (lastSlashPos > 0)
                {
                    apiName = this.ConnectorId.Substring(lastSlashPos + 1);
                }

                return apiName;
            }
        }
    }
}
