// <copyright file="FlowConnectionMap.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

namespace WARP.XrmSolutionAssistant.Core.Models
{
    /// <summary>
    /// Container for a Flow Connection api to target connection.
    /// </summary>
    internal class FlowConnectionMap
    {
        /// <summary>
        /// Gets or sets the Api Name.
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// Gets or sets the Connection Unique Name in the target.
        /// </summary>
        public string ConnectionUniqueName { get; set; }
    }
}
