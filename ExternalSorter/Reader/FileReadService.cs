using ExternalSorter.Model;

namespace ExternalSorter.Reader
{
    internal class FileReadService
    {
        public async Task<List<ElementPortionModel>> OpenAndReadFiles(string[] files, int maxLineToLoad)
        {
            var result = new List<ElementPortionModel>();
            foreach (var file in files)
            {
                var streamReader = new StreamReader(file);
                var streamReaderInfo = new StreamReaderInfo { Reader = streamReader, EndOfFile = false, FileName = file };
                var readCount = 0;
                while (readCount < maxLineToLoad && !streamReader.EndOfStream)
                {
                    var elementModel = new ElementPortionModel() { StreamReaderInfo = streamReaderInfo };
                    var line = await streamReader.ReadLineAsync();
                    if (line == null)
                    {
                        continue;
                    }
                    elementModel.Element = new Element(line);
                    result.Add(elementModel);
                    readCount++;
                }
                if (streamReader.EndOfStream)
                {
                    streamReaderInfo.EndOfFile = true;
                }
            }
            return result;
        }
    }
}
