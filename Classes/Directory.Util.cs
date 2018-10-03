using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace PSExtend.Directory
{
    public static class Util
    {

        public static (IList<FileSystemInfo> fsInfo, List<Exception> exceptions) GetChildFiles(string Path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);
            IList<FileSystemInfo> fileList = new List<FileSystemInfo>();
            List<Exception> exceptions = new List<Exception>();

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
                exceptions.Add(UAex);
            }
            
            return (fsInfo: fileList, exceptions: exceptions);
        }

        public static (DirectorySizeInfo dsInfo, ConcurrentQueue<Exception> exceptions) GetDirectorySize(string Path)
        {
            long directorySize = 0;
            long fileCount = 0;
            long directoryCount = 0;
            long reparsePointCount = 0;
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);

            // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
            var exceptions = new ConcurrentQueue<Exception>();

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
                    FileAttributes attributes = System.IO.File.GetAttributes(subDir.FullName);
                    
                    if ((attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    {
                        System.Threading.Interlocked.Add(ref reparsePointCount, 1);
                    }
                    else {
                        var dirSizeInfo = GetDirectorySize(subDir.FullName);
                        foreach (Exception exception in dirSizeInfo.exceptions)
                        {
                            exceptions.Enqueue(exception);
                        }
                        System.Threading.Interlocked.Add(ref directorySize, dirSizeInfo.dsInfo.DirectorySize);
                        System.Threading.Interlocked.Add(ref fileCount, dirSizeInfo.dsInfo.FileCount);
                        System.Threading.Interlocked.Add(ref directoryCount, dirSizeInfo.dsInfo.DirectoryCount);
                        System.Threading.Interlocked.Add(ref reparsePointCount, dirSizeInfo.dsInfo.ReparsePointCount);
                    }
                    
                });
            }
            catch (System.UnauthorizedAccessException UAex)
            {
                exceptions.Enqueue(UAex);
            }
            catch (System.IO.PathTooLongException PTLex)
            {
                exceptions.Enqueue(PTLex);   
            }
            catch (System.AggregateException Aex)
            {
                exceptions.Enqueue(Aex);
            }

            DirectorySizeInfo sizeInfo = new DirectorySizeInfo {
                DirectorySize = directorySize,
                FileCount = fileCount,
                DirectoryCount = directoryCount,
                ReparsePointCount = reparsePointCount
            };
            
            return (dsInfo: sizeInfo, exceptions: exceptions);
        }

        public static (IList<FileSystemInfo> fsInfo, ConcurrentQueue<Exception> exceptions) GetCurrentDirectories(string Path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);
            IList<FileSystemInfo> directoryList = new List<FileSystemInfo>();

            var dirSizeInfo = GetDirectorySize(directoryInfo.FullName);
                
            if (dirSizeInfo.dsInfo.DirectorySize >= 0)
            {
                directoryList.Add(new FileSystemInfo {
                    FullName = directoryInfo.FullName,
                    Name = directoryInfo.Name,
                    Size = dirSizeInfo.dsInfo.DirectorySize,
                    FileCount = dirSizeInfo.dsInfo.FileCount,
                    DirectoryCount = dirSizeInfo.dsInfo.DirectoryCount,
                    IsDirectory = true,
                    ReparsePointCount = dirSizeInfo.dsInfo.ReparsePointCount
                });
            }
            
            return (fsInfo: directoryList, exceptions: dirSizeInfo.exceptions);
        }

        public static (IList<FileSystemInfo> fsInfo, ConcurrentQueue<Exception> exceptions) GetChildDirectories(string Path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);
            IList<FileSystemInfo> directoryList = new List<FileSystemInfo>();

            var exceptions = new ConcurrentQueue<Exception>();

            try {
                DirectoryInfo[] directories = directoryInfo.GetDirectories();

                System.Threading.Tasks.Parallel.ForEach(directories, (subdir) => {
                    var dirSizeInfo = GetDirectorySize(subdir.FullName);
                    foreach (Exception exception in dirSizeInfo.exceptions)
                    {
                        exceptions.Enqueue(exception);
                    }
                    
                    if (dirSizeInfo.dsInfo.DirectorySize >= 0)
                    {
                        directoryList.Add(new FileSystemInfo {
                            FullName = subdir.FullName,
                            Name = subdir.Name,
                            Size = dirSizeInfo.dsInfo.DirectorySize,
                            FileCount = dirSizeInfo.dsInfo.FileCount,
                            DirectoryCount = dirSizeInfo.dsInfo.DirectoryCount,
                            IsDirectory = true,
                            ReparsePointCount = dirSizeInfo.dsInfo.ReparsePointCount
                        });
                    }
                });
            }
            catch (System.UnauthorizedAccessException UAex)
            {
                exceptions.Enqueue(UAex);
            }
            catch (System.IO.PathTooLongException PTLex)
            {
                exceptions.Enqueue(PTLex);
            }
            catch (System.AggregateException Aex)
            {
                exceptions.Enqueue(Aex);
            }
            
            return (fsInfo: directoryList, exceptions: exceptions);
        }

    }
}