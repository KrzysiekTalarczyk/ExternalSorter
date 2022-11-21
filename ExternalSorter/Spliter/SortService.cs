using ExternalSorter.Model;
using ExternalSorter.Merge;
using ExternalSorter.Sorter;
using ExternalSorter.Writer;
using System.Collections.Concurrent;
using System.Text;
using ExternalSorter.Reader;

namespace ExternalSorter.Spliter
{
    internal class SortService
    {
        private readonly IFileMergeService _fileMergeService;
        private BlockingCollection<MergeFileStage> _mergedFilesQueue;
        private ConcurrentBag<Task<string>> _mergeTasks;

        public SortService()
        {
            _mergeTasks = new ConcurrentBag<Task<string>>();
            _mergedFilesQueue = new BlockingCollection<MergeFileStage>();
            _fileMergeService = new FileMergeService(_mergedFilesQueue, new FileReadService(), _mergeTasks);
        }

        private const int MaxByteCount = 1024 * 1024 * Configurations.MAX_SINGLE_READED_MB;

        public async Task<string> StartWorking(string inputFilePath)
        {
            using StreamReader sourceFile = new StreamReader(inputFilePath);
            var fileMerger = Task.Run(() => _fileMergeService.StartMergingFile(sourceFile));
            int byteCount = 0;
            int splitedFileNumber = 0;
            int mergedSplitedFileNumber = 0;
            var splitFileTasks = new List<Task<string>>();

            while (!sourceFile.EndOfStream)
            {
                var cashe = new ElementSrtedList();
                while (byteCount < MaxByteCount && !sourceFile.EndOfStream)
                {
                    var line = sourceFile.ReadLine();
                    if (line == null)
                        continue;
                    byteCount += Encoding.UTF8.GetByteCount(line);
                    cashe.Insert(line);
                }
                byteCount = 0;
                await CheckMememory();
                splitFileTasks.Add(SaveSortedFile(splitedFileNumber, cashe));
                splitedFileNumber++;
                if (splitedFileNumber % Configurations.MAX_FIRST_STAGE_MERGE_FILES == 0)
                {
                    _mergeTasks.Add(MergeSortedFiles(splitFileTasks, mergedSplitedFileNumber++));
                    splitFileTasks.Clear();
                }
            }
            Console.WriteLine("End read base input file.");
            if(splitFileTasks.Count > 0)
                _mergeTasks.Add(MergeSortedFiles(splitFileTasks, mergedSplitedFileNumber++));
            Task.WaitAll(_mergeTasks.ToArray());
            Console.WriteLine("End first stage merge.");
            var resullt = await fileMerger;
            Console.WriteLine("End last stage merge.");
            sourceFile.Dispose();
            return resullt;
        }
  
        private Task<string> MergeSortedFiles(List<Task<string>> splitFileTasks, int number)
        {
            var tasks = Task.WhenAll(splitFileTasks).ContinueWith(results =>
            {
                return _fileMergeService.HandleMerge(results.Result, number, 1);
            });
            return tasks.Result;
        }

        private async Task CheckMememory()
        {
            var memoryInfo = GC.GetGCMemoryInfo();
            var freeMemory = memoryInfo.TotalAvailableMemoryBytes - GC.GetTotalMemory(false);
            while (freeMemory < MaxByteCount)
            {
                await Task.Delay(1000);
            }
        }

        private Task<string> SaveSortedFile(int number, ElementSrtedList cashe)
        {
            return Task.Run(() =>
             {
                 cashe.SortedList.Sort((x, y) => x.CompareTo(y));
                 return FileWriter.WriteToFileAsync(number, cashe);
             });
        }
    }
}
