using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Xunit;
using Path = System.IO.Path;

namespace Vim.UnitTest
{
    public abstract class FileSystemTest : IDisposable
    {
        private Dictionary<string, string> _savedEnvVariables;
        private FileSystem _fileSystemRaw;
        private IFileSystem _fileSystem;

        public FileSystemTest()
        {
            _fileSystemRaw = new FileSystem();
            _fileSystem = _fileSystemRaw;
            _savedEnvVariables = new Dictionary<string, string>();

            foreach (var name in _fileSystem.EnvironmentVariables.SelectMany(ev => ev.Split(new[] { '%' }, StringSplitOptions.RemoveEmptyEntries)))
            {
                _savedEnvVariables[name] = Environment.GetEnvironmentVariable(name);
                Environment.SetEnvironmentVariable(name, null);
            }
        }

        public virtual void Dispose()
        {
            foreach (var pair in _savedEnvVariables)
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }

        public sealed class EncodingTest : FileSystemTest
        {
            private readonly string _tempFilePath;

            public EncodingTest()
            {
                _tempFilePath = System.IO.Path.GetTempFileName();
            }

            public override void Dispose()
            {
                base.Dispose();
                File.Delete(_tempFilePath);
            }

            /// <summary>
            /// Make sure the encoding detection can handle a file with an umlaut in it that doesn't
            /// have a specific byte order marker.  UTF8 can't handle this correctly and the 
            /// implementation must fall back to a Latin1 encoding
            /// </summary>
            [Fact]
            public void UmlautNoBom()
            {
		var line = "let map = �";
                var encoding = Encoding.GetEncoding("Latin1");
                var bytes = encoding.GetBytes(line);
		File.WriteAllBytes(_tempFilePath, bytes);
                var lines = _fileSystem.ReadAllLines(_tempFilePath).Value;
                Assert.Equal(line, lines[0]);
            }

            [Fact]
            public void UmlautWithBom()
            {
		var line = "let map = �";
                var encoding = Encoding.GetEncoding("Latin1");
                File.WriteAllLines(_tempFilePath, new[] { line }, encoding);
                var lines = _fileSystem.ReadAllLines(_tempFilePath).Value;
                Assert.Equal(line, lines[0]);
            }
        }

        public sealed class MiscTest : FileSystemTest
        {
            [Fact]
            public void GetVimRcDirectories1()
            {
                Assert.Equal(0, _fileSystem.GetVimRcDirectories().Count());
            }

            [Fact]
            public void GetVimRcDirectories2()
            {
                Environment.SetEnvironmentVariable("HOME", @"c:\temp");
                Assert.Equal(@"c:\temp", _fileSystem.GetVimRcDirectories().Single());
            }

            [Fact]
            public void GetVimRcFilePaths1()
            {
                Environment.SetEnvironmentVariable("HOME", @"c:\temp");
                var list = _fileSystem.GetVimRcFilePaths().ToList();
                Assert.Equal(@"c:\temp\.vsvimrc", list[0]);
                Assert.Equal(@"c:\temp\_vsvimrc", list[1]);
                Assert.Equal(@"c:\temp\.vimrc", list[2]);
                Assert.Equal(@"c:\temp\_vimrc", list[3]);
            }

            /// <summary>
            /// If the MYVIMRC environment variable is set then prefer that over the standard
            /// paths
            /// </summary>
            [Fact]
            public void GetVimRcFilePaths_MyVimRc()
            {
                Environment.SetEnvironmentVariable("MYVIMRC", @"c:\temp\.vimrc");
                var filePath = _fileSystem.GetVimRcFilePaths().First();
                Assert.Equal(@"c:\temp\.vimrc", filePath);
                Environment.SetEnvironmentVariable("MYVIMRC", null);
            }

            [Fact]
            public void HomeDrivePathTakesPrecedenceOverUserProfile()
            {
                Environment.SetEnvironmentVariable("HOMEDRIVE", "c:");
                Environment.SetEnvironmentVariable("HOMEPATH", "\\temp");
                Environment.SetEnvironmentVariable("USERPROFILE", "c:\\Users");
                var list = _fileSystem.GetVimRcFilePaths().ToList();
                Assert.Equal(@"c:\temp\.vsvimrc", list[0]);
                Assert.Equal(@"c:\temp\_vsvimrc", list[1]);
                Assert.Equal(@"c:\temp\.vimrc", list[2]);
                Assert.Equal(@"c:\temp\_vimrc", list[3]);
            }
        }
    }
}
