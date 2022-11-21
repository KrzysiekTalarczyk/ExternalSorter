using ExternalSorter;
using ExternalSorter.Spliter;
using System.Diagnostics;

Console.WriteLine("Sorter start work");
var sw = Stopwatch.StartNew();
try
{
    PrepareWorkingDirectory();
    var inputFile = GetInputFilePath();
    var fileSpliter = new SortService();
    var outputFile = await fileSpliter.StartWorking(inputFile);

    Console.WriteLine($"Results of sorting: {Path.GetFullPath(outputFile)}");
    Console.WriteLine($"File was sorted in {sw.Elapsed.TotalSeconds} seconds");
    Console.ReadKey();
}
catch (Exception ex)
{
    throw;
}

void PrepareWorkingDirectory()
{
    if (Directory.Exists(Configurations.WorkingDirectory))
        Directory.Delete(Configurations.WorkingDirectory, true);
    Directory.CreateDirectory(Configurations.WorkingDirectory);
}

string GetInputFilePath()
{
    var arg = Environment.GetCommandLineArgs();
    if (arg.Length < 2)
        throw new Exception("Incorrect parameter!");
    var file = arg[1];
    if (!File.Exists(file))
        throw new Exception($"The file {file} not exist!");
    return file;
}
