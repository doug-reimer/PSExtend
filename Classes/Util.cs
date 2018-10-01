using System.Collections.Generic;
using System.IO;

namespace PSExtend
{
    public static class Util
    {

        public static IList<FileSystemInfo> GetChildFiles(string Path)
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
            catch (System.UnauthorizedAccessException)
            {
                // throw(new System.UnauthorizedAccessException());
            }
            
            return fileList;
        }

        public static (long DirectorySize, long FileCount, long DirectoryCount) GetDirectorySize(string Path)
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
                // Carry on
                // throw(new System.UnauthorizedAccessException());
            }
            catch (System.IO.PathTooLongException)
            {
                // Carry on
                // throw(new System.IO.PathTooLongException());
            }
            catch (System.AggregateException)
            {
                // Carry on
                // throw(new System.AggregateException());
            }
            

            return (DirectorySize: directorySize, FileCount: fileCount, DirectoryCount: directoryCount);
        }

    }
}