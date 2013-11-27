module OrganizeFiles

open System
open System.Drawing
open System.IO
open System.Text
open System.Text.RegularExpressions
open Microsoft.FSharp.Collections

let dateTakenFromExif file =
  let r = new Regex(":")
  use fs = new FileStream(file, FileMode.Open, FileAccess.Read)
  use myImage = Image.FromStream(fs, false, false)
  let propItem = myImage.GetPropertyItem(36867)
  let dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2)
  DateTime.Parse(dateTaken)

let getDateTaken file =
  try dateTakenFromExif file
  with _ -> File.GetLastWriteTime(file)   //Use the last write time in the event that the file was moved/copied

let getNewFilename newFilePath =
  let rec loop file n =
    if File.Exists(file) then
      let f2 =
        match n with
          | 2 ->
            let fileDir = Path.GetDirectoryName(file)
            let nextFile = Path.GetFileNameWithoutExtension(file) + "-" + n.ToString() + Path.GetExtension(file)
            Path.Combine(fileDir, nextFile)
          | _ -> 
            let prev = n-1
            file.Replace("-" + prev.ToString(), "-" + n.ToString())
      
      loop f2 (n+1)
    else
      file

//      let fileDir = Path.GetDirectoryName(file)
//      let nextFile = Path.GetFileNameWithoutExtension(file) + "-" + n.ToString() + Path.GetExtension(file)
//      loop (Path.Combine(fileDir, nextFile)) (n+1)

  loop newFilePath 2

let move destinationRoot files =
    let moveHelper file =
        let dateTaken = getDateTaken file
        let finalPath = Path.Combine(destinationRoot, dateTaken.Year.ToString(), dateTaken.ToString("yyyy-MM-dd"))
        if not(Directory.Exists(finalPath)) then Directory.CreateDirectory(finalPath) |> ignore

        let newFile = getNewFilename (Path.Combine(finalPath, Path.GetFileName(file)))

        try File.Copy(file, newFile)
        with e -> failwith (sprintf "error renaming %s to %s\n%s" file newFile e.Message)
    
    files |> PSeq.iter moveHelper


let moveFrom source destination =
  Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)
    |> Seq.filter (fun f -> Path.GetExtension(f).ToLower() <> ".db")  //exlcude the thumbs.db files
    |> move destination
  printfn "Done"
