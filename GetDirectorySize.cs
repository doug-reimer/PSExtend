using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Linq;

namespace PSExtend
{
    [Cmdlet(VerbsCommon.Get, "DirectorySize")]
    [OutputType(typeof(FileSystemInfo))]
    public class GetDirectorySizeCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = false,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("FullName")]
        public string Path { get; set; }

        private string GetWorkingDirectory()
        {
            return CurrentProviderLocation("FileSystem").ProviderPath;
        }

        protected override void BeginProcessing()
        {
            WriteVerbose("Begin!");
            if (Path is null)
            {
                Path = GetWorkingDirectory();
            }
        }

        // This method will be called for each input received from the pipeline to 
        // this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            FileAttributes attributes = System.IO.File.GetAttributes(Path);
            
            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                IEnumerable<FileSystemInfo> childDirs = GetChildDirectories(Path).AsEnumerable();
                IList childDirList = childDirs.OrderBy(n => n.Name).ToList();

                foreach (FileSystemInfo dir in childDirList)
                {
                    WriteObject(dir);
                }
            }
            else
            {
                FileInfo file = new FileInfo(Path);
                WriteObject(new FileSystemInfo {
                        FullName = file.FullName,
                        Name = file.Name,
                        Size = file.Length,
                        IsDirectory = false
                    });
            }
            
        }

        private IList<FileSystemInfo> GetChildDirectories(string Path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);
            IList<FileSystemInfo> directoryList = new List<FileSystemInfo>();

            try
            {
                var DirSizeInfo = Util.GetDirectorySize(directoryInfo.FullName);
                    
                    if (DirSizeInfo.DirectorySize >= 0)
                    {
                        directoryList.Add(new FileSystemInfo {
                            FullName = directoryInfo.FullName,
                            Name = directoryInfo.Name,
                            Size = DirSizeInfo.DirectorySize,
                            FileCount = DirSizeInfo.FileCount,
                            DirectoryCount = DirSizeInfo.DirectoryCount,
                            IsDirectory = true
                        });
                    }
            }
            catch (System.UnauthorizedAccessException UAex)
            {
                WriteWarning(UAex.Message);
            }
            catch (System.IO.PathTooLongException PTLex)
            {
                WriteWarning(PTLex.Message);
            }
            catch (System.AggregateException Aex)
            {
                WriteWarning(Aex.Message);
            }
            
            return directoryList;
        }

        // This method will be called once at the end of pipeline execution; 
        // if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
