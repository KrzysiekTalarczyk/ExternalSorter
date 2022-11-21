using ExternalSorter.Model;
using ExternalSorter.Reader;
using ExternalSorter.Writer;
using System.Collections.Concurrent;
using System.Text;

namespace ExternalSorter.Merge
{
    internal class FileMergeService : IFileMergeService
    {
        private readonly IFileReadService _fileReadService;
        private BlockingCollection<MergeFileStage> _mergedFilesQueue;
        private ConcurrentBag<Task<string>> _mergeTasks;

        public FileMergeService(BlockingCollection<MergeFileStage> mergedFilesQueue,
                                IFileReadService fileReadService,
                                ConcurrentBag<Task<string>> mergeTasks)
        {
            _mergedFilesQueue = mergedFilesQueue;
            _fileReadService = fileReadService;
            _mergeTasks = mergeTasks;
        }

        public Task<string> HandleMerge(string[] files, int number, int stage)
        {
            var mergeTask = Task.Run(async () =>
             {
                 var mergedFile = await MergeFiles(files, number, stage);
                 _mergedFilesQueue.Add(new MergeFileStage(mergedFile, stage));
                 return mergedFile;
             });
            return mergeTask;
        }

        private async Task<string> MergeFiles(string[] files, int number, int stage)
        {
            var maxLineToLoad = Configurations.LINES_LOADED_TO_MERGE * stage;
            BlockingCollection<string> linesToWrite = new BlockingCollection<string>();
            var elements = await _fileReadService.OpenAndReadFiles(files, maxLineToLoad);
            var openFiles = new List<StreamReaderInfo>();
            elements.Select(x => x.StreamReaderInfo).Distinct().ToList().ForEach(x => openFiles.Add((x)));
            var writer = Task.Run(() => FileWriter.StartWriting(linesToWrite, number, stage));
            var readedFile = 0;
            do
            {
                elements.Sort((x, y) => x.Element.CompareTo(y.Element));
                var elementToSave = elements.Take(maxLineToLoad).ToList();
                elementToSave.ForEach(x => elements.Remove(x));

                AddToQueue(linesToWrite, elementToSave);

                var FilesToReRead = elementToSave.Where(x => x.StreamReaderInfo.EndOfFile == false)
                                                 .GroupBy(x => x.StreamReaderInfo.FileName)
                                                 .Select(x => new { FileName = x.Key, ElementModel = x.First(), Count = x.Count() });

                foreach (var item in FilesToReRead)
                {
                    var readCount = 0;
                    while (readCount < item.Count && !item.ElementModel.StreamReaderInfo.Reader.EndOfStream)
                    {
                        var line = await item.ElementModel.StreamReaderInfo.Reader.ReadLineAsync();
                        if (line == null)
                        {
                            continue;
                        }
                        item.ElementModel.Element = new Element(line);
                        if (item.ElementModel.Element == null)
                        {
                            throw new Exception("empty");
                        }
                        elements.Add(item.ElementModel);
                        readCount++;
                    }
                    if (item.ElementModel.StreamReaderInfo.Reader.EndOfStream)
                    {
                        item.ElementModel.StreamReaderInfo.EndOfFile = true;
                        readedFile++;
                    }
                }
            } while (readedFile < files.Count() - 1);
            var lastOpenFile = openFiles.Where(x => x.EndOfFile == false).Single();
            var lastElements = await ReadLastFileToEnd(lastOpenFile);
            AddToQueue(linesToWrite, elements);
            linesToWrite.Add(lastElements);
            linesToWrite.CompleteAdding();
            var results = await writer;
            DeleteFile(openFiles);
            return results;
        }


        private async Task<string> ReadLastFileToEnd(StreamReaderInfo lastOpenFile)
        {
            var lastLines = await lastOpenFile.Reader.ReadToEndAsync();
            return lastLines;
        }

        private void DeleteFile(IEnumerable<StreamReaderInfo> files)
        {
            foreach (var file in files)
            {
                try
                {
                    file.Reader.Close();
                    File.Delete(file.FileName);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<string> StartMergingFile(StreamReader sourceFile)
        {
            var mergeCount = 0;

            var files = new List<MergeFileStage>();
            foreach (var file in _mergedFilesQueue.GetConsumingEnumerable())
            {
                files.Add(file);
                var groupByStage = files.GroupBy(x => x.Stage);
                foreach (var stage in groupByStage)
                {
                    if (stage.Count() == CalculateMergeFileCount(stage.Key))
                    {
                        var mergeTask = HandleMerge(stage.Select(f => f.File).ToArray(), mergeCount++, stage.Key + 1);
                        _mergeTasks.Add(mergeTask);
                        foreach (var item in stage)
                        {
                            files.Remove(item);
                        }
                    }
                }
                if (ContinueTask(sourceFile))
                    continue;
                else
                {
                    if (files.Count == 1)
                        break;
                    var temp = new List<MergeFileStage>();
                    files.ForEach(f => temp.Add(f));
                    var nextStage = files.Max(x => x.Stage) + 1;
                    files.Clear();
                    temp.ForEach(f => f.Stage = nextStage);
                    temp.ForEach(f => _mergedFilesQueue.Add(f));
                }
            }
            var inputFile = files.First().File;
            return inputFile;
        }

        private int CalculateMergeFileCount(int stage)
        {
            var result = stage switch
            {
                > 2 => 2,
                _ => Configurations.MAX_LAST_STAGE_MERGE_FILES,
            };
            return result < 2 ? 2 : result;
        }

        private bool ContinueTask(StreamReader sourceFile)
        {
            var mergequeueNotEmpty = _mergedFilesQueue.Count > 0;
            var isActiveMergeTask = _mergeTasks.Any(x => x.IsCompleted == false);
            var shouldContinue = isActiveMergeTask || !sourceFile.EndOfStream || mergequeueNotEmpty;
            return shouldContinue;
        }

        private void AddToQueue(BlockingCollection<string> linesToWrite, List<ElementPortionModel> elementToSave)
        {
            var sb = new StringBuilder();
            elementToSave.ForEach(x => sb.AppendLine(x.Element.ToString()));
            linesToWrite.Add(sb.ToString());
        }
    }
}
