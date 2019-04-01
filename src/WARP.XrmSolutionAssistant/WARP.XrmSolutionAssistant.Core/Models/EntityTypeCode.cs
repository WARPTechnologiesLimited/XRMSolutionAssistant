// <copyright file="EntityTypeCode.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

namespace WARP.XrmSolutionAssistant.Core.Models
{
    /// <summary>
    /// A class to represent the entity and corresponding type code.
    /// </summary>
    internal class EntityTypeCode
    {
        /// <summary>
        /// Gets or sets the logical name of the entity. Lowercase.
        /// </summary>
        public string EntityLogicalName { get; set; }

        /// <summary>
        /// Gets or sets the value for Entity Type Code (AKA Object Type Code).
        /// </summary>
        public int TypeCode { get; set; }
    }
}
