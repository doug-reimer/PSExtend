namespace PSExtend
{
    public class FileSystemInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public long? Size { get; set; }
        public long? FileCount { get; set; }
        public long? DirectoryCount {get; set; }
        public bool IsDirectory { get; set; }
        public long? ReparsePointCount { get; set; }
    }
}