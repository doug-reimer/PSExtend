using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Linq;

namespace PSExtend
{
    [Cmdlet(VerbsCommon.Get, "ChildItemSize")]
    [OutputType(typeof(FileSystemInfo))]
    public class GetChildItemSizeCommand : PSCmdlet
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
        // This method gets called once for each cmdlet in the pipeline when the 
        // pipeline starts executing
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

                foreach (FileSystemInfo file in GetChildFiles(Path))
                {
                    WriteObject(file);
                }
            }
            
        }


        private (long DirectorySize, long FileCount, long DirectoryCount) GetDirectorySize(string Path)
        {
            long directorySize = 0;
            long fileCount = 0;
            long directoryCount = 0;
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);

            try
            {
                FileInfo[] files = directoryInfo.GetFiles();

                foreach (FileInfo file in files)
                {
                    System.Threading.Interlocked.Add(ref directorySize, file.Length);
                }
                System.Threading.Interlocked.Add(ref fileCount, files.Length);

                DirectoryInfo[] directories = directoryInfo.GetDirectories();
                System.Threading.Interlocked.Add(ref directoryCount, directories.Length);

                System.Threading.Tasks.Parallel.ForEach(directories, (subDir) => {
                    
                    var DirSizeInfo = GetDirectorySize(subDir.FullName);
                    System.Threading.Interlocked.Add(ref directorySize, DirSizeInfo.DirectorySize);
                    System.Threading.Interlocked.Add(ref fileCount, DirSizeInfo.FileCount);
                    System.Threading.Interlocked.Add(ref directoryCount, DirSizeInfo.DirectoryCount);
                });
            }
            catch (System.UnauthorizedAccessException)
            {
                directorySize = -1;
            }
            catch (System.IO.PathTooLongException)
            {
                directorySize = -1;
            }
            catch (System.AggregateException)
            {
                directorySize = -1;
            }
            

            return (DirectorySize: directorySize, FileCount: fileCount, DirectoryCount: directoryCount);
        }

        private IList<FileSystemInfo> GetChildDirectories(string Path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);
            IList<FileSystemInfo> directoryList = new List<FileSystemInfo>();

            try {
                DirectoryInfo[] directories = directoryInfo.GetDirectories();

                System.Threading.Tasks.Parallel.ForEach(directories, (subdir) => {
                    var DirSizeInfo = GetDirectorySize(subdir.FullName);
                    
                    if (DirSizeInfo.DirectorySize >= 0)
                    {
                        directoryList.Add(new FileSystemInfo {
                            FullName = subdir.FullName,
                            Name = subdir.Name,
                            Size = DirSizeInfo.DirectorySize,
                            FileCount = DirSizeInfo.FileCount,
                            DirectoryCount = DirSizeInfo.DirectoryCount,
                            IsDirectory = true
                        });
                    }
                    
                });
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

        private IList<FileSystemInfo> GetChildFiles(string Path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);
            IList<FileSystemInfo> fileList = new List<FileSystemInfo>();

            try {
                FileInfo[] files = directoryInfo.GetFiles();

                foreach (FileInfo file in files)
                {
                    fileList.Add(new FileSystemInfo {
                        FullName = System.IO.Path.Combine(Path, file.Name),
                        Name = file.Name,
                        Size = file.Length,
                        IsDirectory = false
                    });
                }
            }
            catch (System.UnauthorizedAccessException UAex)
            {
                WriteWarning(UAex.Message);
            }
            
            return fileList;
        }

        // This method will be called once at the end of pipeline execution; 
        // if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
