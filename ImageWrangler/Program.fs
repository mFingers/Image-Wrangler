﻿open OrganizeFiles

module Program =
  [<EntryPoint>]
  let main args =
    let destination = """C:\_EXTERNAL_DRIVE\_Camera2"""

    moveFrom """C:\Users\Mike\Pictures\To Network\104___09"""  destination
//    moveFrom """C:\_EXTERNAL_DRIVE\Camera"""
    0