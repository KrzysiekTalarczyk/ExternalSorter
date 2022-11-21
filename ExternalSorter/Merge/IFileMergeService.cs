
namespace ExternalSorter.Merge
{
    internal interface IFileMergeService
    {
        Task<string> HandleMerge(string[] files, int number, int stage);
        Task<string> StartMergingFile(StreamReader sourceFile);
    }
}