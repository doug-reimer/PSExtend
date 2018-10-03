# PSExtensions

A cross-platform module containing Cmdlets that extend functionality within PowerShell.

### Cmdlets
```powershell
Get-ChildItemSize [[-Path] <String>] [<CommonParameters>]
```
A multi-threaded cmdlet to retrieve file and directory sizes.
```powershell
Get-DirectorySize [[-Path] <String>] [<CommonParameters>]
```
A multi-threaded cmdlet to retrieve the size of a specified directory.

### Installation
Copy the PSExtend directory from this repository to the PowerShell module path:
```powershell 
$env:PSModulePath
```

Import the PSExtend module:
```powershell
Import-Module PSExtend
```

### Examples
```powershell
PS C:\> Get-DirectorySize -Path C:\Temp\Applications
```
```
Name          Size     FileCount DirectoryCount
----          ----     --------- --------------
Applications  3.806 GB 11        5             
```

```powershell
PS C:\> Get-ChildItemSize -Path /tmp
```
```
Name          Size       FileCount DirectoryCount
----          ----       --------- --------------
CM13          499.183 MB 2         0             
Images        1.225 GB   3         0             
LineageOS     490.955 MB 1         0             
OpenApps      1.545 GB   2         0             
RecoveryIMG   45.249 MB  2         0             
notes.txt     10.244 KB                          
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


