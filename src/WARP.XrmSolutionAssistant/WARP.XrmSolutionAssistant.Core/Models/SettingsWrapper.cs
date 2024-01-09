﻿// <copyright file="SettingsWrapper.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

namespace WARP.XrmSolutionAssistant.Core.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Class representing the structure of the settings JSON file.
    /// </summary>
    internal class SettingsWrapper
    {
        /// <summary>
        /// Gets or sets the collection of entity type codes.
        /// </summary>
        public List<EntityTypeCode> EntityTypeCodes { get; set; }

        /// <summary>
        /// Gets or sets solutionVersionReset.
        /// </summary>
        public SolutionVersionReset SolutionVersionReset { get; set; }

        /// <summary>
        /// Gets or sets FlowConnectionMapper.
        /// </summary>
        public FlowConnectionMapper FlowConnectionMapper { get; set; }

        /// <summary>
        /// Gets or sets AssemblyAndStepVersionReset.
        /// </summary>
        public AssemblyAndStepVersionReset AssemblyAndStepVersionReset { get; set; }
    }
}
