namespace DPCClient.Model
{
    public class NLogMessage
    {
        public string Message { get; set; }
        public string Time { get; set; }
        public string Level { get; set; }
        public int RowCount { get; set; }
        public string MethodName { get; set; }
        public string ConflictMethodName { get; set; }
        public int ConflictRow { get; set; }
    }
}
