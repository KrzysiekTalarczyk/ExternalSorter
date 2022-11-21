using ExternalSorter.Model;

namespace ExternalSorter.Reader
{
    internal interface IFileReadService
    {
        Task<List<ElementPortionModel>> OpenAndReadFiles(string[] files, int maxLineToLoad);
    }
}