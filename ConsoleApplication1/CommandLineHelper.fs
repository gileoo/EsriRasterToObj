module CommandLineHelper

open System


let inline optionValue (argv) (optName) (parse:string -> 'a) (noVal:'a) = 
    let optIdx =
        argv
        |> Array.tryFindIndex( (=) ("-" + optName) )
    
    if optIdx.IsSome && argv.Length > optIdx.Value + 1 then
        parse argv.[optIdx.Value + 1]
    else
        noVal


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