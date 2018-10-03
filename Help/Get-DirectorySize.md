---
external help file: PSExtend.dll-Help.xml
Module Name: PSExtend
online version:
schema: 2.0.0
---

# Get-DirectorySize

## SYNOPSIS
Get the size of a directory.

## SYNTAX

```
Get-DirectorySize [[-Path] <String>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DirectorySize cmdlet retrieves the size, file and directory count of a specified directory.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DirectorySize
```

This command gets the size of the current directory.

### Example 2
```powershell
PS C:\> Get-DirectorySize -Path C:\Temp
```

This command gets the size, directory and file count of the C:\Temp directory.

## PARAMETERS

### -Path
Specifies a path to a location. The default location is the current directory.

```yaml
Type: String
Parameter Sets: (All)
Aliases: FullName

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable.
For more information, see about_CommonParameters (http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### PSExtend.FileSystemInfo
## NOTES

## RELATED LINKS
