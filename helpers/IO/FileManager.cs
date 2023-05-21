using ByteSizeLib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Linq;
using helpers.Extensions;

namespace helpers.IO
{
    [LogSource("File Manager")]
    public class FileManager 
    {
        private FileInfo _file;
        private FileVersionInfo _fv;
        private DirectoryManager _directory;

        public static IReadOnlyList<char> PathIllegalCharacters { get; } = new char[]
        {
            '/',
            '<',
            '>',
            ':',
            '"',
            '\\',
            '|',
            '?',
            '*'
        };

        public static IReadOnlyList<string> ReservedFileNames { get; } = new string[]
        {
            "CON",
            "PRN",
            "AUX",
            "NUL",
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "LPT1",
            "LPT2",
            "LPT3",
            "LPT4",
            "LPT5",
            "LPT6",
            "LPT7",
            "LPT8",
            "LPT9"
        };

        public FileManager(string filePath)
        {
            _file = new FileInfo(filePath);

            if (!_file.Exists) _file.Create().Close();

            _fv = FileVersionInfo.GetVersionInfo(filePath);
            _directory = new DirectoryManager(_file.DirectoryName);
        }

        internal FileManager(FileInfo fileInfo)
        {
            _file = fileInfo;
            _directory = new DirectoryManager(fileInfo.DirectoryName);
            _fv = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
        }

        public FileInfo Info { get => _file; }
        public FileVersionInfo VersionInfo { get => _fv; }

        public string Extension { get => _file.Extension; }
        public string Path { get => _file.FullName; }
        public string Name { get => _file.Name; }
        public string OriginalName { get => _fv.OriginalFilename; }
        public string InternalName { get => _fv.InternalName; }
        public string Hash { get => CalculateHash(Path); }
        public string Comments { get => _fv.Comments; }
        public string CompanyName { get => _fv.CompanyName; }
        public string ProductName { get => _fv.ProductName; }
        public string Language { get => _fv.Language; }
        public string Copyright { get => _fv.LegalCopyright; }
        public string Trademark { get => _fv.LegalTrademarks; }
        public string FileVersion { get => _fv.FileVersion; }
        public string ProductVersion { get => _fv.ProductVersion; }
        public string SpecialBuildData { get => _fv.SpecialBuild; }
        public string PrivateBuildData { get => _fv.PrivateBuild; }

        public int FileVersionMajor { get => _fv.FileMajorPart; }
        public int FileVersionMinor { get => _fv.FileMinorPart; }
        public int FileVersionBuild { get => _fv.FileBuildPart; }
        public int FileVersionPrivate { get => _fv.FilePrivatePart; }
        
        public int ProductVersionMajor { get => _fv.ProductMajorPart; }
        public int ProductVersionMinor { get => _fv.ProductMinorPart; }
        public int ProductVersionBuild { get => _fv.ProductBuildPart; }
        public int ProductVersionPrivate { get => _fv.ProductPrivatePart; }

        public long Size { get => _file.Length; }

        public bool Exists { get => _file.Exists; }
        public bool IsReadOnly { get => _file.IsReadOnly; set => _file.IsReadOnly = value; }
        public bool IsCompressed { get => _file.Attributes.HasFlag(FileAttributes.Compressed); }
        public bool IsEncrypted { get => _file.Attributes.HasFlag(FileAttributes.Encrypted); }
        public bool IsTemporary { get => _file.Attributes.HasFlag(FileAttributes.Temporary); }
        public bool IsHidden { get => _file.Attributes.HasFlag(FileAttributes.Hidden); }
        public bool IsSystemFile { get => _file.Attributes.HasFlag(FileAttributes.System); }
        public bool IsDebug { get => _fv.IsDebug; }
        public bool IsPatched { get => _fv.IsPatched; }
        public bool IsPreRelease { get => _fv.IsPreRelease; }
        public bool IsPrivateBuild { get => _fv.IsPrivateBuild; }
        public bool IsSpecialBuild { get => _fv.IsSpecialBuild; }
        public bool IsAssembly { get => Extension.Contains("dll"); }

        public DateTime CreationTime { get => _file.CreationTime; set => File.SetCreationTime(Path, value); }
        public DateTime LastAccessTime { get => _file.LastAccessTime; set => File.SetLastAccessTime(Path, value); }
        public DateTime LastWriteTime { get => _file.LastWriteTime; set => File.SetLastWriteTime(Path, value); }

        public DirectoryManager Directory { get => _directory; }

        public void EncryptFile()
        {
            _file.Encrypt();
        }

        public void DecryptFile()
        {
            _file.Decrypt();
        }

        public void Delete()
        {
            _file.Delete();
        }

        public void Move(string path)
        {
            _file.MoveTo(path);
        }

        public FileManager Copy(string destination)
        {
            return new FileManager(_file.CopyTo(destination));
        }

        public StreamWriter OpenWriter()
        {
            return new StreamWriter(OpenFile());
        }

        public StreamReader OpenReader()
        {
            return new StreamReader(OpenFile());
        }

        public FileStream OpenFile()
        {
            return _file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public void Create(bool overwrite = false)
        {
            if (!Exists || overwrite)
                File.Create(Path)
                    .Close();
        }

        public void WriteBytes(byte[] bytes)
        {
            File.WriteAllBytes(Path, bytes);
        }

        public void WriteLines(IEnumerable<string> lines)
        {
            File.WriteAllLines(Path, lines);
        }

        public void WriteLines(string[] lines)
        {
            File.WriteAllLines(Path, lines);
        }

        public void WriteLine(string line)
        {
            File.WriteAllLines(Path, new string[] { line });
        }

        public void WriteText(string text)
        {
            File.WriteAllText(Path, text);
        }

        public byte[] ReadBytes()
        {
            return File.ReadAllBytes(Path);
        }

        public IEnumerable<string> ReadLines()
        {
            return File.ReadAllLines(Path);
        }

        public string ReadText()
        {
            return File.ReadAllText(Path);
        }

        public double GetSizeInKiloBytes()
        {
            var size = ByteSize.FromBytes(Size);

            return Math.Round(size.KiloBytes);
        }

        public double GetSizeInMegaBytes()
        {
            var size = ByteSize.FromBytes(Size);

            return Math.Round(size.MegaBytes);
        }

        public double GetSizeInGigaBytes()
        {
            var size = ByteSize.FromBytes(Size);

            return Math.Round(size.GigaBytes);
        }

        public double GetSizeInPetaBytes()
        {
            var size = ByteSize.FromBytes(Size);

            return Math.Round(size.PetaBytes);
        }

        public string GetSizeString()
        {
            var pb = GetSizeInPetaBytes();

            if (pb > 0)
                return $"{pb} PB";

            var gb = GetSizeInGigaBytes();

            if (gb > 0)
                return $"{gb} GB";

            var mb = GetSizeInMegaBytes();

            if (mb > 0)
                return $"{mb} MB";

            var kb = GetSizeInKiloBytes();

            if (kb > 0)
                return $"{kb} KB";

            return $"{Size} B";
        }

        public static bool IsValidFileName(string fileName)
        {
            if (fileName == null)
                return false;

            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            if (ReservedFileNames.Contains(fileName))
                return false;

            if (PathIllegalCharacters.Any(x => fileName.Contains(x)))
                return false;

            return true;
        }

        public static bool IsValidPath(string path)
        {
            if (path is null)
                return false;

            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (path.EndsWith("."))
                return false;

            return true;
        }

        public static string FixFilePath(string input)
        {
            return input
                .RemovePathUnsafe()
                .Trim();
        }

        public static string CalculateHash(string path)
        {
            using (var md = MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                var hash = md.ComputeHash(stream);

                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }
    }
}
