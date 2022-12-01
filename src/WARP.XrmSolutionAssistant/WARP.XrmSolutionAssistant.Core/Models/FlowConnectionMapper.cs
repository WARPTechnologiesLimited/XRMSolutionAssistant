// <copyright file="FlowConnectionMapper.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

namespace WARP.XrmSolutionAssistant.Core.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Model of information required for aligning Flow connections.
    /// </summary>
    internal class FlowConnectionMapper
    {
        /// <summary>
        /// Gets or sets the list of FlowConnectionMaps.
        /// </summary>
        public List<FlowConnectionMap> Maps { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to throw an exception for unmapped references.
        /// </summary>
        public bool ThrowExceptionOnUnmappedConnections { get; set; }
    }
}
