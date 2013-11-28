open OrganizeFiles
open System.Configuration

module Program =
  [<EntryPoint>]
  let main args =
    let sourceDir = ConfigurationManager.AppSettings.Get("sourceDirectory")
    let destDir = ConfigurationManager.AppSettings.Get("destinationDirectory")

    moveFrom sourceDir  destDir
    0