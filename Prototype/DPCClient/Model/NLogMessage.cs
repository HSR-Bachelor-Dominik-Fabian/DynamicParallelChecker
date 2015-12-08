namespace DPCClient.Model
{
    public class NLogMessage
    {
        public string Message { get; set; }
        public string Time { get; set; }
        public string Level { get; set; }
        public int RowCount { get; set; }

        public string ClassName => GetTypeName(MethodName);
        public string MethodName { get; set; }
        public string ConflictClassName => GetTypeName(ConflictMethodName);
        public string ConflictMethodName { get; set; }
        public int ConflictRow { get; set; }
        public string MethodContent { get; set; }
        public string ConflictContent { get; set; }

        public static string GetTypeName(string methodName)
        {
            if (methodName != null)
            {
                string[] parts = methodName.Split(' ');
                string[] parts2 = parts[parts.Length - 1].Split(':');
                return parts2[0];
            }
            return null;
        }
    }
}
