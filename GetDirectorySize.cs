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
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("FullName")]
        public string Path { get; set; }

        // This method gets called once for each cmdlet in the pipeline when the 
        // pipeline starts executing
        protected override void BeginProcessing()
        {
            WriteVerbose("Begin!");
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


        private long GetDirectorySize(string Path)
        {
            long directorySize = 0;
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);

            try
            {
                FileInfo[] files = directoryInfo.GetFiles();

                foreach (FileInfo file in files)
                {
                    System.Threading.Interlocked.Add(ref directorySize, file.Length);
                }

                DirectoryInfo[] directories = directoryInfo.GetDirectories();

                System.Threading.Tasks.Parallel.ForEach(directories, (subDir) => {
                    
                    long size = GetDirectorySize(subDir.FullName);
                    System.Threading.Interlocked.Add(ref directorySize, size);
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
            

            return directorySize;
        }

        private IList<FileSystemInfo> GetChildDirectories(string Path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);
            IList<FileSystemInfo> directoryList = new List<FileSystemInfo>();

            try {
                DirectoryInfo[] directories = directoryInfo.GetDirectories();

                System.Threading.Tasks.Parallel.ForEach(directories, (subdir) => {
                    long size = GetDirectorySize(subdir.FullName);
                    
                    if (size >= 0)
                    {
                        directoryList.Add(new FileSystemInfo {
                            FullName = subdir.FullName,
                            Name = subdir.Name,
                            Size = size,
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
