﻿// <copyright file="Program.cs" company="WARP Technologies Limited">
// Copyright © WARP Technologies Limited
// </copyright>

namespace WARP.XrmSolutionAssistant.Runner
{
    using System;

    /// <summary>
    /// Application to tidy CRM changes pulled down from CRM in a multi developer environment
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

            var rootDirectory = args[0];

            var entityAligner = new SolutionEntityAligner(rootDirectory);
            entityAligner.Execute();

            var sorter = new SolutionXmlSorter(rootDirectory);
            sorter.Execute();

            var workflowGuidAligner = new SolutionWorkflowGuidAligner(rootDirectory);
            workflowGuidAligner.Execute();

            Console.WriteLine();
            Console.WriteLine("Complete. Press any key to close.");
            Console.ReadKey();
        }
    }
}