[![Build Status](https://warpuk.visualstudio.com/XRMSolutionAssistant/_apis/build/status/XRMSolutionAssistant-CI?branchName=master)](https://warpuk.visualstudio.com/XRMSolutionAssistant/_build/latest?definitionId=72&branchName=master)
# XRMSolutionAssistant
A .NET Standard assembly offering tooling to assist with the management of exported Microsoft CRM solution files and reducing noise in a multi-developer, branched environment.
## Available Tools
- Entity OTC Aligner
- Version Reset
- Workflow Guid Aligner
- XML Sorter
### Overview
#### Entity OTC Aligner
For versions of CRM < 9, each custom entity was assigned a code upon installation. Between different CRM Organizations, this value would be different. Even if sourced from the same solution. This tool allows the code to be set to a known value when extracted.
#### Version Reset
Many items within a CRM solution retain the version that they were initially installed with on a developer instance. To provide predictable, consistent source control files, this tool will reset this to a defined value in the Solution.xml and any other file where the ``<IntroducedVersion>`` tag is found.
#### Workflow Guid Aligner
Under some circumstances, the xaml for a Workflow may contain ``<Variable x:...>`` elements with Guids that are generated upon installation and therefore different every time the solution is extracted. This tool will replace those Guids with a predictable value.
#### XML Sorter
Due to the nature of the *SolutionPackager.exe* tool, nested elements may be written in an unpredictable order and create noise in source control change. This tool will alpha order those elements to maintain consistency across different extracts.
## Usage
### General
All the tools are contained in the *WARP.XrmSolutionAssistant.dll* which may be called from a console application.

All the tools are in the `` WARP.SolutionAssistant `` namespace.

All the tools work on a folder structure that has been created from the *SolutionPackager.exe* console application from the CRM SDK.

Alongside the assembly is ``settings.json`` that provides information to some of the tooling.
### XRMSolutionAssistant.Console
XRMSolutionAssistant.Console is an example .NET Core console application which may be used for a quick-start and implements all the tools in the assistant assembly. The full stand-alone Windows 64-bit build is available from the releases tab [here](https://github.com/WARPTechnologiesLimited/XRMSolutionAssistant/releases). Usage:
```
WARP.XRMSolutionAssistant.Console.exe /<folder>
```
Where *folder* is the path to the extracted solution file. Folder can be either a full or relative path.

To prevent a particular tool from running, locate the ``appsettings.json`` file in the same directory as the executable and edit the `Excludes` property which is a list of names of tools that you wish to exclude from running.
```javascript
{
  "Excludes": [ "SolutionWorkflowGuidAligner", "SolutionVersionReset" ]
}
```

### Entity OTC Aligner
Modify your ``settings.json`` to contain a collection of ``EntityTypeCodes`` as below:
```javascript
{
  "EntityTypeCodes": [
    {
      "EntityLogicalName": "warp_customentity1",
      "TypeCode": 10029
    },
    {
      "EntityLogicalName": "warp_customentity2",
      "TypeCode": 10028
    }
  ]
}
```
Implementation
```csharp
            var entityAligner = new SolutionEntityAligner(folder);
            entityAligner.Execute();
```
### Version Reset
Modify your ``settings.json`` SolutionVersionReset member to reflect the version you want the solution components to align to.
```javascript
{
  "EntityTypeCodes": [
    {
      "EntityLogicalName": "warp_customentity",
      "TypeCode": 10145
    }
  ],
  "SolutionVersionReset": {
    "ResetVersion":  "1.0.0.0" 
  } 
}
```

Implementation
```csharp
            var solutionVersionResetter = new SolutionVersionReset(folder);
            solutionVersionResetter.Execute();
```
### Workflow Guid Aligner
Implementation
```csharp
            var workflowGuidAligner = new SolutionWorkflowGuidAligner(folder);
            workflowGuidAligner.Execute();
```
### XML Sorter
Implementation
```csharp
            var sorter = new SolutionXmlSorter(folder);
            sorter.Execute();
```
=======
