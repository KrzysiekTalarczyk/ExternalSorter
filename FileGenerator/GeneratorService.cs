using System.Collections.Concurrent;
using System.Text;

namespace FileGenerator
{
    internal class GeneratorService
    {

        private BlockingCollection<byte[]> _portionQueue;
        private const int DUPLICATION_OCCURRENCE = 4;
        private int bufferMaxSize = 10 * 1024 * 1024;
        private readonly long _expectedBytes;
        private readonly string _path = @$".\randomElements_{DateTime.UtcNow.Ticks}.dat";
        private readonly Random _random;
        private readonly ILineGenerator _lineGenerator;

        public GeneratorService(uint sizeMB, ILineGenerator lineGenerator)
        {
            _random = new Random();
            _portionQueue = new BlockingCollection<byte[]>();
            _expectedBytes = 1024 * 1024 * sizeMB;
            _lineGenerator = lineGenerator;
        }

        public async Task<string> GenerateFile()
        {
            List<Task<byte[]>> tasks = new List<Task<byte[]>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(GetPortion());
            }
            var writer = StartWriteToFile();
            while (tasks.Count > 0)
            {
                var t = await Task.WhenAny(tasks);
                tasks.Remove(t);
                if (!TryAddToQueue(await t))
                    break;
                tasks.Add(GetPortion());
            }
            return await writer;
        }

        private Task<string> StartWriteToFile()
        {
            return Task.Run(async () =>
            {
                var currentSize = 0;
                using (FileStream file = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.None, bufferMaxSize))
                {
                    while (currentSize < _expectedBytes)
                    {
                        var portion = _portionQueue.Take();
                        await file.WriteAsync(portion, 0, portion.Length);
                        currentSize += portion.Length;
                    }
                }
                _portionQueue.CompleteAdding();
                return _path;
            });
        }

        private bool TryAddToQueue(byte[] portion)
        {
            if (!_portionQueue.IsAddingCompleted)
            {
                _portionQueue.Add(portion);
                return true;
            }
            return false;
        }

        private Task<byte[]> GetPortion()
        {
            return Task.Run(() =>
            {
                var bytes = (_random.Next(0, 10) % DUPLICATION_OCCURRENCE) != 0 ?
                     GetRandomElements()
                     : GetRepetedElements();
                return bytes;
            });
        }

        private byte[] GetRepetedElements()
        {
            var batch = new List<byte>();
            var element = _lineGenerator.GenereteLine();
            var bytes = Encoding.UTF8.GetBytes(element);
            var allBytes = 0;
            while (allBytes < bufferMaxSize / 2)
            {
                batch.AddRange(bytes);
                allBytes += bytes.Length;
            }
            return batch.ToArray();
        }

        private byte[] GetRandomElements()
        {
            var batch = new List<byte>();
            var allBytes = 0;
            while (allBytes < bufferMaxSize)
            {
                var element = _lineGenerator.GenereteLine();
                var bytes = Encoding.UTF8.GetBytes(element);
                batch.AddRange(bytes);
                allBytes += bytes.Length;
            }
            return batch.ToArray();
        }
    }
}
