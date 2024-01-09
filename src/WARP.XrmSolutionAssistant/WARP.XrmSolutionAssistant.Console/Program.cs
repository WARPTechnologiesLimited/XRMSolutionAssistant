// <copyright file="Program.cs" company="WARP Technologies Limited">
// Released by WARP for use by the CRM development community.
// </copyright>

// ReSharper disable CheckNamespace
namespace WARP.XrmSolutionAssistant.Core.Console
{
    using System;
    using System.IO;
    using System.Linq;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Application to tidy CRM changes pulled down from CRM in a multi developer environment.
    /// </summary>
    public class Program
    {
        private static void Main(string[] args)
        {
            if (args is null || args.Length < 1)
            {
                Console.WriteLine("Please specify path.");
                return;
            }

            var excludes = new string[0];
            var executingDirectory = AppContext.BaseDirectory;
            Console.WriteLine($"Looking for appsettings.json in: {executingDirectory}");
            if (File.Exists(Path.Combine(executingDirectory, "appsettings.json")))
            {
                var builder = new ConfigurationBuilder().SetBasePath(executingDirectory).AddJsonFile("appsettings.json", true, true);

                var configurationRoot = builder.Build();
                var config = configurationRoot.Get<Config>();
                excludes = config.Excludes;
                Console.WriteLine($"Excludes: {string.Join(",", excludes)}");
            }
            else
            {
                Console.WriteLine($"No appsettings.json file found. Executing all assistants.");
            }

            var rootDirectory = args[0];

            if (!excludes.Contains(nameof(SolutionEntityAligner)))
            {
                var entityAligner = new SolutionEntityAligner(rootDirectory);
                entityAligner.Execute();
            }

            if (!excludes.Contains(nameof(SolutionXmlSorter)))
            {
                var sorter = new SolutionXmlSorter(rootDirectory);
                sorter.Execute();
            }

            if (!excludes.Contains(nameof(SolutionWorkflowGuidAligner)))
            {
                var workflowGuidAligner = new SolutionWorkflowGuidAligner(rootDirectory);
                workflowGuidAligner.Execute();
            }

            if (!excludes.Contains(nameof(SolutionVersionReset)))
            {
                var solutionVersionResetter = new SolutionVersionReset(rootDirectory);
                solutionVersionResetter.Execute();
            }

            if (!excludes.Contains(nameof(AssemblyAndStepVersionReset)))
            {
                var assemblyVersionReset = new AssemblyAndStepVersionReset(rootDirectory);
                assemblyVersionReset.Execute();
            }

            if (!excludes.Contains(nameof(SolutionFlowConnectionMapper)))
            {
                var flowMapper = new SolutionFlowConnectionMapper(rootDirectory);
                flowMapper.Execute();
            }

            Console.WriteLine();
            Console.WriteLine("Complete");
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    /// <summary>
    /// Container for app config items.
    /// </summary>
    internal class Config
    {
        /// <summary>
        /// Gets or sets the assistants to exclude from running.
        /// </summary>
        public string[] Excludes { get; set; }
    }
}
