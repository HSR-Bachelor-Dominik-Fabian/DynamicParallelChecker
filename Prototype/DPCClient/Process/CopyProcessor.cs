
using System.IO;
using DPCClient.Model;

namespace DPCClient.Process
{
    class CopyProcessor
    {
        private static readonly string _targetDirectory = "work";
        private static readonly string _targetDirectoryDecompile = "work_decompile";

        public string Start(FilePathModel filePath)
        {
            var directory = Path.GetDirectoryName(filePath.FilePath);
            var fileName = Path.GetFileName(filePath.FilePath);

            if (Directory.Exists(_targetDirectory))
            {
                Delete(_targetDirectory);
            }
            if (Directory.Exists(_targetDirectoryDecompile))
            {
                Delete(_targetDirectoryDecompile);
            }

            Directory.CreateDirectory(_targetDirectory);
            Directory.CreateDirectory(_targetDirectoryDecompile);

            Copy(directory, _targetDirectory);

            Copy(directory, _targetDirectoryDecompile);

            CopyLibrary();

            return _targetDirectory + @"\" + fileName;
        }

        public void CleanUp()
        {
            Delete(_targetDirectory);
        }

        private void CopyLibrary()
        {
            var libraryFileName = "DPCLibrary.dll";
            File.Copy(libraryFileName, _targetDirectory + @"\" + libraryFileName);

            var nLogFileName = "NLog.dll";
            File.Copy(nLogFileName, _targetDirectory + @"\" + nLogFileName);

            var nLogConfigFileName = "NLog.config";
            File.Copy(nLogConfigFileName, _targetDirectory + @"\" + nLogConfigFileName);
        }

        private void Copy(string directory, string targetDirectory, string relative = "")
        {
            if (directory != null)
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    var fileName = Path.GetFileName(file);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        var pathDirectory = Path.Combine(targetDirectory, relative);
                        if (!Directory.Exists(pathDirectory))
                        {
                            Directory.CreateDirectory(pathDirectory);
                        }
                        File.Copy(file, Path.Combine(pathDirectory, fileName), true);
                    }
                }
                foreach (var directoryTemp in Directory.GetDirectories(directory))
                {
                    var dirName = new DirectoryInfo(directoryTemp).Name;
                    Copy(directoryTemp, Path.Combine(relative, dirName));
                }
            }
        }

        private void Delete(string directory)
        {
            if (directory != null)
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    File.Delete(file);
                }
                foreach (var directoryTemp in Directory.GetDirectories(directory))
                {
                    Delete(directoryTemp);
                    Directory.Delete(directoryTemp);
                }
                Directory.Delete(directory);
            }
        }
    }
}
