namespace DPCClient.Model
{
    class LogEntryModel
    {
        public string FilePath { get; set; }

        public LogEntryModel(string filePath)
        {
            FilePath = filePath;
        }
    }
}
