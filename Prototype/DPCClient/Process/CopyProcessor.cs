
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
            string directory = Path.GetDirectoryName(filePath.FilePath);
            string fileName = Path.GetFileName(filePath.FilePath);

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
            Delete(_targetDirectoryDecompile);
        }

        private void CopyLibrary()
        {
            string libraryFileName = "DPCLibrary.dll";
            File.Copy(libraryFileName, _targetDirectory + @"\" + libraryFileName);

            string nLogFileName = "NLog.dll";
            File.Copy(nLogFileName, _targetDirectory + @"\" + nLogFileName);

            string nLogConfigFileName = "NLog.config";
            File.Copy(nLogConfigFileName, _targetDirectory + @"\" + nLogConfigFileName);
        }

        private void Copy(string directory, string targetDirectory, string relative = "")
        {
            if (directory != null)
            {
                foreach (string file in Directory.GetFiles(directory))
                {
                    string fileName = Path.GetFileName(file);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        string pathDirectory = Path.Combine(targetDirectory, relative);
                        if (!Directory.Exists(pathDirectory))
                        {
                            Directory.CreateDirectory(pathDirectory);
                        }
                        File.Copy(file, Path.Combine(pathDirectory, fileName), true);
                    }
                }
                foreach (string directoryTemp in Directory.GetDirectories(directory))
                {
                    string dirName = new DirectoryInfo(directoryTemp).Name;
                    Copy(directoryTemp, Path.Combine(relative, dirName));
                }
            }
        }

        private void Delete(string directory)
        {
            if (directory != null)
            {
                foreach (string file in Directory.GetFiles(directory))
                {
                    File.Delete(file);
                }
                foreach (string directoryTemp in Directory.GetDirectories(directory))
                {
                    Delete(directoryTemp);
                    Directory.Delete(directoryTemp);
                }
                Directory.Delete(directory);
            }
        }
    }
}
