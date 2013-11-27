open System
open System.Drawing
open System.IO
open System.Text
open System.Text.RegularExpressions


let actOnFiles directory fileOp =
    let rec getFilesHelper files dir =
        Directory.EnumerateDirectories(dir)
            |> Seq.collect (fun s -> getFilesHelper files s)
            |> Seq.append (Directory.EnumerateFiles(dir))

    getFilesHelper Seq.empty directory |> fileOp


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
//        printfn "rename %s to %s" file newFile
        File.Copy(file, newFile)

    files |> Seq.iter moveHelper


//let sourceDir = """C:\_EXTERNAL_DRIVE\Camera"""
let sourceDir = """C:\Users\Mike\Pictures\To Network"""
let sourceDir = """C:\_EXTERNAL_DRIVE\Camera\3rd Birthday"""
let sourceDir = """C:\_EXTERNAL_DRIVE\Camera"""
let destDir = """C:\_EXTERNAL_DRIVE\_Camera"""

try
    actOnFiles sourceDir (move destDir)
    printfn "Done"
with
| :? Exception as e -> printfn "ERROR: %s" e.Message

actOnFiles """C:\_EXTERNAL_DRIVE\Camera\""" (Seq.map (fun f -> Path.GetExtension(f))) |> Seq.distinct |> Seq.toList
//
//let aFile = """C:\_EXTERNAL_DRIVE\Camera\_latest\DSC03953.JPG"""
//let aFile = """C:\_EXTERNAL_DRIVE\Camera\_latest\MOV04924.MPG"""
//let dateTaken = aFile |> getDateTaken
//Path.Combine(destinationRoot, dateTaken.Year.ToString(), dateTaken.ToString("yyyy-MM-dd"))
