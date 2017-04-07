Code source from:
https://github.com/wixtoolset/wix4

Oh yeah!


// CREATING ZIP FILE BY ADDING LIST OF FILES
ZipInfo zip = new ZipInfo(@"D:\testarchive1.zip");
var files = new List<string>();
files.Add(@"D:\outils\defraggler x86.exe");
files.Add(@"C:\Windows\Notepad.exe");
zip.PackFiles(null, files, null);

//// CREATING ZIP FILE BY ADDING FOLDER (WITH SUB-FOLDERS) USING MAXIMUM COMPRESSION
//zip = new ZipInfo(@"C:\testarchive2.zip");
//zip.Pack(@"C:\Test", true, Microsoft.Deployment.Compression.CompressionLevel.Max, null);


// CREATING CAB FILE BY ADDING LIST OF FILES
CabInfo cab = new CabInfo(@"D:\testarchive1.cab");
files.Clear();
files.Add(@"C:\Windows\Explorer.exe");
files.Add(@"C:\Windows\Notepad.exe");
cab.PackFiles(null, files, null);

// CREATING CAB FILE BY ADDING FOLDER (WITH SUB-FOLDERS) USING MINIMUM COMPRESSION
//cab = new CabInfo(@"D:\testarchive2.cab");
//cab.Pack(@"C:\Balaji", true, Microsoft.Deployment.Compression.CompressionLevel.Min, null);