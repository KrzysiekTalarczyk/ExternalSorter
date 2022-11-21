#for generate 1000 MB files with random elements. 
.\FileGenerator.exe 1000
#Generator use SampleLine.txt as a dictionary of sample line.
#You can use your own sample lines. Set the path to your sample lines file as the second parameter e.g.:

.\FileGenerator.exe 1000 MySampleLines.txt

#for sorting file randomElements.dat

.\ExternalSorter.exe randomElements.dat
