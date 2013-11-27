open System
open System.Drawing
open System.IO
open System.Text
open System.Text.RegularExpressions

let logFile = new StreamWriter("""log.txt""")
let logOp msg file =    
    logFile.WriteLine(file + "\n" + msg) |> ignore


let actOnFiles directory fileOp =
    let rec getFilesHelper files dir =
        Directory.EnumerateDirectories(dir)
            |> Seq.collect (fun s -> getFilesHelper files s)
            |> Seq.append (Directory.EnumerateFiles(dir))

    (getFilesHelper Seq.empty directory) |> fileOp


let dateTakenFromExif file =
    let r = new Regex(":")
    use fs = new FileStream(file, FileMode.Open, FileAccess.Read)
    use myImage = Image.FromStream(fs, false, false)
    let propItem = myImage.GetPropertyItem(36867)
    let dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2)
    DateTime.Parse(dateTaken)

let getDateTaken file =
    try
        dateTakenFromExif file
    with
    | :? Exception ->
        logOp "Obtaining date from last write time" file
        File.GetLastWriteTime(file)   //Use the last write time in the event that the file was moved/copied


let getNewFilename oldFileName =
    let rec incrementFilename file n =
        let fileDir = Path.GetDirectoryName(file)
        let nextFile = Path.GetFileNameWithoutExtension(file) + "-" + n.ToString() + Path.GetExtension(file)
        let filenameIncremented = Path.Combine(fileDir, nextFile)
        //printfn "fileDir: %s" fileDir
        match File.Exists(filenameIncremented) with
        | false -> filenameIncremented
        | true -> incrementFilename filenameIncremented (n+1)

    match File.Exists(oldFileName) with
    | false -> oldFileName
    | true  -> incrementFilename oldFileName 2

let move destinationRoot files =
    let moveHelper file =
        let dateTaken = getDateTaken file
        let dest = Path.Combine(destinationRoot, dateTaken.Year.ToString(), dateTaken.ToString("yyyy-MM-dd"))
        Directory.CreateDirectory(dest) |> ignore

        let newFile = getNewFilename (Path.Combine(dest, Path.GetFileName(file)))
        file |> logOp ("Copy to " + newFile)
        File.Copy(file, newFile)

    files |> Seq.iter moveHelper


let sourceDir = """C:\_EXTERNAL_DRIVE\Camera"""
//let sourceDir = """C:\Users\Mike\Pictures\To Network"""
//let sourceDir = """C:\_EXTERNAL_DRIVE\Camera"""
let destDir = """C:\_EXTERNAL_DRIVE\_Camera"""

try
    actOnFiles sourceDir (move destDir)
    printfn "Done"
with
| :? Exception as e ->
    logFile.WriteLine("ERROR: " + e.Message) |> ignore

logFile.Flush()
logFile.Close()

logFile.Dispose()