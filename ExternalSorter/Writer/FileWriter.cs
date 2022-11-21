using ExternalSorter.Sorter;
using System.Collections.Concurrent;
using System.Text;

namespace ExternalSorter.Writer
{
    internal class FileWriter
    {
        public static async Task<string> WriteToFileAsync(int number, ElementSrtedList content)
        {
            string outputFileName = $"SortedElements_{number}_{DateTime.UtcNow.Ticks}.dat";
            var outputFilePath = Path.Combine(Configurations.WorkingDirectory, outputFileName);
            int bufferSize = 1024 * 1024 * Configurations.BUFFER_SIZE;
            await using var stream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write,
                                                    FileShare.None, bufferSize, FileOptions.SequentialScan);
            await using var streamWriter = new StreamWriter(stream);
            var sb = new StringBuilder();
            foreach (var element in content.SortedList)
            {
                sb.AppendLine(element.ToString());
            }
            await streamWriter.WriteAsync(sb.ToString());
            streamWriter.Dispose();
            stream.Close();
            return outputFilePath;
        }

        public static async Task<string> StartWriting(BlockingCollection<string> queue, int number, int stage)
        {
            var outputFileName = $"MergedElements_{number}_Stage_{stage}_{DateTime.UtcNow.Ticks}.dat";
            var path = Path.Combine(Configurations.WorkingDirectory, outputFileName);
            while (queue.IsCompleted == false)
            {
                var sb = new StringBuilder();
                var counter = 0;
                foreach (var line in queue.GetConsumingEnumerable())
                {
                    sb.Append(line.ToString());
                    counter++;
                    if (counter > Configurations.LINES_PORTION_LOADED_TO_WRITE)
                    {
                        break;
                    }
                }
                await File.AppendAllTextAsync(path, sb.ToString());
            }
            return path;
        }
    }
}