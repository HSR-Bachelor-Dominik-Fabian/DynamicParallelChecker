
using System.IO;
using DPCClient.Model;

namespace DPCClient.Process
{
    class CopyProcessor
    {
        private static readonly string _targetDirectory = "work";

        public string Start(FilePathModel filePath)
        {
            string directory = Path.GetDirectoryName(filePath.FilePath);
            string fileName = Path.GetFileName(filePath.FilePath);

            Directory.CreateDirectory("work");

            Copy(directory, fileName, _targetDirectory);

            CopyLibrary();

            return _targetDirectory + @"\" + fileName;
        }

        public void CleanUp()
        {
            Delete(_targetDirectory);
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

        private void Copy(string directory, string fileName, string copyDirectory)
        {
            if (directory != null)
            {
                foreach (string file in Directory.GetFiles(directory))
                {
                    File.Copy(file, _targetDirectory + @"\" + Path.GetFileName(file));
                }
                foreach (string directoryTemp in Directory.GetDirectories(directory))
                {
                    Copy(directoryTemp, fileName, copyDirectory);
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
