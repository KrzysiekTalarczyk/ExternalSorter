using FileGenerator;
using System.Diagnostics;

Console.WriteLine("File generator start works.");
var sw = Stopwatch.StartNew();
try
{
    var options = GetOptions();
    var lineGenerator = new LineGenerator(options.SampleLinePath);
    var generator = new GeneratorService(options.Size, lineGenerator);
    var path = await generator.GenerateFile();
    Console.WriteLine($"Your file location: [{Path.GetFullPath(path)}]");
    Console.WriteLine($"File was genereted in {sw.Elapsed.TotalSeconds} seconds");
}
catch (Exception ex)
{
    throw;
}

(uint Size, string SampleLinePath) GetOptions()
{
    var arg = Environment.GetCommandLineArgs();
    if (arg.Length < 2)
        throw new Exception("Incorrect parameter!");
    var size = arg[1];
    if (!uint.TryParse(size, out uint sizeMB))
        throw new Exception($"Incorrect size parameter! {size}");
    var path = (arg.Length >= 3) ? arg[2] : "LinesSample.txt";
    if (!File.Exists(path))
        throw new Exception($"Sample lines file {path} not exist.");
    return (sizeMB, path);
}