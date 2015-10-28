using System.ComponentModel;

namespace DPCClient.Model
{
    class LogEntryModel
    {
        [DisplayName(@"Pfad")]
        public string FilePath { get; set; }

        public LogEntryModel(string filePath)
        {
            FilePath = filePath;
        }
    }
}
