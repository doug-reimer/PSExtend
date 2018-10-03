---
external help file: PSExtend.dll-Help.xml
Module Name: PSExtend
online version:
schema: 2.0.0
---

# Get-ChildItemSize

## SYNOPSIS
Get directory and files sizes of child items.

## SYNTAX

```
Get-ChildItemSize [[-Path] <String>] [<CommonParameters>]
```

## DESCRIPTION
The Get-ChildItemSize cmdlet get the sizes of items in one or more specified locations. If the item is a container it gets the items inside the container, known as child items.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-ChildItemSize
```

This command gets the sizes of child items in the current location.  If the location is a file system directory, it gets the files and sub-directories in the current directory.

### Example 2
```powershell
PS C:\> Get-ChildItemSize -Path C:\Temp
```

This command gets the sizes of the child items under the C:\Temp directory.

### Example 3
```powershell
PS /Users/Demo> Get-ChildItemSize /Users/Demo
```

This command gets the sizes of the child items under the /Users/Guest directory.

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
