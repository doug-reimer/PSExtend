namespace PSExtend
{
    public struct DirectorySizeInfo
    {
        public long DirectorySize { get; set; }
        public long FileCount { get; set; }
        public long DirectoryCount { get; set; }
        public long ReparsePointCount { get; set; }
    }

}