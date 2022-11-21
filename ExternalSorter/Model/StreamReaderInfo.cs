namespace ExternalSorter.Model
{
    public class StreamReaderInfo
    {
        public string FileName { get; set; }
        public StreamReader Reader { get; set; }
        public bool EndOfFile { get; set; }
    }
}
