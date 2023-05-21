using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace helpers.IO
{
    [LogSource("Directory Manager")]
    public class DirectoryManager 
    {
        private DirectoryInfo _dirInfo;

        public DirectoryManager(string directoryPath) 
        {
            _dirInfo = new DirectoryInfo(directoryPath);

            Parent = new DirectoryManager(_dirInfo.Parent.FullName);
            Root = new DirectoryManager(_dirInfo.Root.FullName);
        }

        static DirectoryManager()
        {
            Current = new DirectoryManager(Directory.GetCurrentDirectory());
            System = new DirectoryManager(Environment.SystemDirectory);

            var dict = new Dictionary<Environment.SpecialFolder, DirectoryManager>();

            foreach (var enumValue in Enum
                .GetValues(typeof(Environment.SpecialFolder))
                .Cast<Environment.SpecialFolder>())
            {
                dict[enumValue] = Get(enumValue);
            }

            Directories = dict;
        }

        public static IReadOnlyDictionary<Environment.SpecialFolder, DirectoryManager> Directories { get; }

        public static DirectoryManager Current { get; }
        public static DirectoryManager System { get; }
        public static DirectoryManager Roaming { get; } = Directories[Environment.SpecialFolder.ApplicationData];
        public static DirectoryManager Local { get; } = Directories[Environment.SpecialFolder.LocalApplicationData];

        public string Path { get => _dirInfo.FullName; }
        public string Name { get => _dirInfo.Name; }

        public bool Exists { get => _dirInfo.Exists; }

        public DateTime LastWriteTime { get => _dirInfo.LastWriteTime; set => Directory.SetLastWriteTime(Path, value); }
        public DateTime LastAccessTime { get => _dirInfo.LastAccessTime; set => Directory.SetLastAccessTime(Path, value); }
        public DateTime CreationTime { get => _dirInfo.CreationTime; set => Directory.SetCreationTime(Path, value); }

        public DirectoryManager Parent { get; } 
        public DirectoryManager Root { get; }

        public List<DirectoryManager> GetDirectories()
        {
            return _dirInfo.EnumerateDirectories().Select(x => new DirectoryManager(x.FullName)).ToList();
        }

        public List<DirectoryManager> GetDirectories(string searchPattern)
        {
            return _dirInfo.EnumerateDirectories(searchPattern).Select(x => new DirectoryManager(x.FullName)).ToList();
        }

        public List<DirectoryManager> GetDirectories(string searchPattern, SearchOption searchOption)
        {
            return _dirInfo.EnumerateDirectories(searchPattern, searchOption).Select(x => new DirectoryManager(x.FullName)).ToList();
        }

        public List<FileManager> GetFiles()
        {
            return _dirInfo.EnumerateFiles().Select(x => new FileManager(x.FullName)).ToList();
        }

        public List<FileManager> GetFiles(string searchPattern)
        {
            return _dirInfo.EnumerateFiles(searchPattern).Select(x => new FileManager(x.FullName)).ToList();
        }

        public List<FileManager> GetFiles(string searchPattern, SearchOption searchOption) 
        {
            return _dirInfo.EnumerateFiles(searchPattern, searchOption).Select(x => new FileManager(x.FullName)).ToList();
        }

        public void Create()
        {
            if (!Exists)
                _dirInfo.Create();
        }

        public void Delete()
        {
            if (Exists)
                _dirInfo.Delete(true);
        }

        public void CreateSubdirectory(string path)
        {
            _dirInfo.CreateSubdirectory(path);
        }

        public void Move(string path)
        {
            _dirInfo.MoveTo(path);
        }

        public override string ToString() => Path;

        public static DirectoryManager Get(Environment.SpecialFolder folder)
        {
            return new DirectoryManager(Environment.GetFolderPath(folder));
        }
    }
}
