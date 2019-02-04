// <copyright file="SettingsWrapper.cs" company="WARP Technologies Limited">
// Copyright © WARP Technologies Limited
// </copyright>

namespace WARP.XrmSolutionAssistant.Models
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
    }
}
