namespace FileGenerator
{
    internal class LineGenerator : ILineGenerator
    {
        private readonly List<string> _sampleLines;
        private readonly Random _random;
        private const int MIN_NUMBER = 1;
        private const int MAX_NUMBER = 100000;

        public LineGenerator(string sampleLinesPath)
        {
            _sampleLines = LoadSampleLines(sampleLinesPath);
            _random = new Random();
        }

        private List<string> LoadSampleLines(string sampleLinesPath)
        {
            var lines = new List<string>();
            foreach (string line in File.ReadLines(sampleLinesPath))
            {
                lines.Add(line);
            }
            return lines;
        }

        public string GenereteLine()
        {
            var text = _sampleLines[_random.Next(0, _sampleLines.Count - 1)];
            var number = _random.Next(MIN_NUMBER, MAX_NUMBER);
            return $"{number}.{text}{Environment.NewLine}";
        }
    }
}
