#load "PSeq.fs"
#load "renamePics.fs"

open OrganizeFiles

let destination = """C:\_EXTERNAL_DRIVE\_Camera2"""

#time
moveFrom """C:\Users\Mike\Pictures\To Network\104___09"""  destination
moveFrom """C:\_EXTERNAL_DRIVE\Camera""" destination
