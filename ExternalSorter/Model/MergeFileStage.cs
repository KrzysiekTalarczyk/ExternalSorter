namespace ExternalSorter.Model
{
    public class MergeFileStage
    {
        public string File { get; set; }
        public int Stage { get; set; }

        public MergeFileStage(string file, int stage)
        {
            File = file;
            Stage = stage;
        }
    }
}
