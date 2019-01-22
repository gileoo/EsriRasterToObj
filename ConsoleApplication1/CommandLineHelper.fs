module CommandLineHelper

open System


/// Search argument list for an option and return its parsed value. If not 
/// found, or the parsing is invalid return the noVal as default.
let inline optionValue (argv) (optName) (parse:string -> 'a) (noVal:'a) = 
    let optIdx =
        argv
        |> Array.tryFindIndex( (=) ("-" + optName) )
    
    if optIdx.IsSome && argv.Length > optIdx.Value + 1 then
        parse argv.[optIdx.Value + 1]
    else
        noVal


/// Apply a task on all files in the current directory. The last element
/// of the argument list is assumed to be a wildcard, or file specifier.
/// The task is a function taking one filePath.
let onCurrentDirectoryFiles (argv:string[]) (task:string -> unit)  =
    if argv.Length > 0 then
        let files =  
            IO.Directory.GetFiles( 
                IO.Directory.GetCurrentDirectory(), 
                argv.[argv.Length-1], 
                IO.SearchOption.TopDirectoryOnly )

        if files.Length = 0 then
            false
        else 
            files |> Array.iter( task )
            printfn "done"
            true
    else
        false