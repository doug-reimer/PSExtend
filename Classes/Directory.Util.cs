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

        public static (DirectorySizeInfo sizeInfo, ConcurrentQueue<Exception> exceptions) GetDirectorySize(string Path)
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
                        var DirSizeInfo = GetDirectorySize(subDir.FullName);
                        foreach (Exception exception in DirSizeInfo.exceptions)
                        {
                            exceptions.Enqueue(exception);
                        }
                        System.Threading.Interlocked.Add(ref directorySize, DirSizeInfo.sizeInfo.DirectorySize);
                        System.Threading.Interlocked.Add(ref fileCount, DirSizeInfo.sizeInfo.FileCount);
                        System.Threading.Interlocked.Add(ref directoryCount, DirSizeInfo.sizeInfo.DirectoryCount);
                        System.Threading.Interlocked.Add(ref reparsePointCount, DirSizeInfo.sizeInfo.ReparsePointCount);
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
            
            return (sizeInfo: sizeInfo, exceptions: exceptions);
        }

        public static (IList<FileSystemInfo> fsInfo, ConcurrentQueue<Exception> exceptions) GetCurrentDirectories(string Path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path);
            IList<FileSystemInfo> directoryList = new List<FileSystemInfo>();

            var dirSizeInfo = Util.GetDirectorySize(directoryInfo.FullName);
                
            if (dirSizeInfo.sizeInfo.DirectorySize >= 0)
            {
                directoryList.Add(new FileSystemInfo {
                    FullName = directoryInfo.FullName,
                    Name = directoryInfo.Name,
                    Size = dirSizeInfo.sizeInfo.DirectorySize,
                    FileCount = dirSizeInfo.sizeInfo.FileCount,
                    DirectoryCount = dirSizeInfo.sizeInfo.DirectoryCount,
                    IsDirectory = true,
                    ReparsePointCount = dirSizeInfo.sizeInfo.ReparsePointCount
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
                    var DirSizeInfo = Util.GetDirectorySize(subdir.FullName);
                    foreach (Exception exception in DirSizeInfo.exceptions)
                    {
                        exceptions.Enqueue(exception);
                    }
                    
                    if (DirSizeInfo.sizeInfo.DirectorySize >= 0)
                    {
                        directoryList.Add(new FileSystemInfo {
                            FullName = subdir.FullName,
                            Name = subdir.Name,
                            Size = DirSizeInfo.sizeInfo.DirectorySize,
                            FileCount = DirSizeInfo.sizeInfo.FileCount,
                            DirectoryCount = DirSizeInfo.sizeInfo.DirectoryCount,
                            IsDirectory = true,
                            ReparsePointCount = DirSizeInfo.sizeInfo.ReparsePointCount
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