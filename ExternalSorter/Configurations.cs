namespace ExternalSorter
{
    public class Configurations
    {
        public const int MAX_SINGLE_READED_MB = 100;

        public const int MAX_LAST_STAGE_MERGE_FILES = 6;

        public const int LINES_LOADED_TO_MERGE = 100;

        public static int MAX_FIRST_STAGE_MERGE_FILES = 5;

        public static int LINES_PORTION_LOADED_TO_WRITE = 100;

        public static string WorkingDirectory = @".\MergedFile";

        public static int BUFFER_SIZE = 50;
    }
}
