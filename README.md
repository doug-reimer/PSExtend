# PSExtensions

This module contains Cmdlets that extend functionality within PowerShell.

### Cmdlets
```powershell
Get-ChildItemSize

Get-DirectorySize
```

### Installation
Copy the PSExtend directory from this repository to the PowerShell module path:
```powershell 
$env:PSModulePath
```

Import the PSExtend module:
```powershell
Import-Module PSExtend
```

### Build Instructions
#### Generate Help Files
Install the Powershell platyPS module to help generate the maml help files.

Instructions on how to use playPS can be found at: https://github.com/PowerShell/platyPS

```powershell
Install-Module -Name platyPS -Scope CurrentUser
Import-Module platyPS
```

Generate the initial Markdown boilerplate for the module:
```powershell
Import-Module PSExtend
New-MarkdownHelp -Module PSExtend -OutputFolder .\Help
```

Modify the generated markdown files, replacing the `"{{ ... }}"` placeholders with help content.

Generate the help files: 
```powershell
New-ExternalHelp .\Help -OutputPath en-US\
```

#### Build
Clone the repository, and run the following command to build the Module:
```
dotnet build
```


