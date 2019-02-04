// <copyright file="SolutionWorkflowGuidAligner.cs" company="WARP Technologies Limited">
// Copyright © WARP Technologies Limited
// </copyright>

namespace WARP.XrmSolutionAssistant
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using NLog;

    /// <summary>
    /// A class to align the Guids within CRM Workflow xml files.
    /// </summary>
    public class SolutionWorkflowGuidAligner
    {
        /// <summary>
        /// NLog reference for this class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string solutionRootDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionWorkflowGuidAligner"/> class.
        /// </summary>
        /// <param name="solutionRootDirectory">Directory containing the </param>
        public SolutionWorkflowGuidAligner(string solutionRootDirectory)
        {
            this.solutionRootDirectory = solutionRootDirectory;
        }

        /// <summary>
        /// Executes the logic to replace workflow Guids with a consistent value.
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
                throw new NotImplementedException();
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

        private static Guid StringToGUID(string value)
        {
            Logger.Trace("String to GUID called for: {0}", value);

            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));

            return new Guid(data);
        }
    }
}
