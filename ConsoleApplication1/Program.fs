open System
open System.IO
open CommandLineHelper


type header =
    {
        cols : int
        rows  : int
        centerX : float
        centerY : float
        cellSize : float 
        noData : float
    }

let headFromArray (x:string[]) =
    {
        cols     = System.Int32.Parse ( x.[0].Split(' ').[1] )
        rows     = System.Int32.Parse ( x.[1].Split(' ').[1] )
        centerX  = System.Double.Parse( x.[2].Split(' ').[1] )
        centerY  = System.Double.Parse( x.[3].Split(' ').[1] )
        cellSize = System.Double.Parse( x.[4].Split(' ').[1] )
        noData   = System.Double.Parse( x.[5].Split(' ').[1] )
    }

type Vec2 =
    {
        X : float
        Y : float
    }
    static member Parse (s:string) =
        let toks = s.Split(',')
        if toks.Length <> 2 then
            failwith (sprintf "Error parsing %s as Vec2" s)
        {
            X = Double.Parse toks.[0]
            Y = Double.Parse toks.[1]
        }

type Vec3 = 
    {
        x : float
        y : float
        z : float
    }

type Bound2 =
    {
        min : Vec2
        max : Vec2
    }
    static member Parse (s:string) =
        let toks = s.Split(':')
        if toks.Length <> 2 then
            failwith (sprintf "Error parsing %s as Bound2" s)
        {
            min = Vec2.Parse toks.[0]
            max = Vec2.Parse toks.[1]
        }


let esriToObj nth shift scale cut fileName =

    printfn "nth: %A, shift: %.1f, %.1f, scale: %.3f - doing: %s" 
        nth shift.X shift.Y scale (Path.GetFileName fileName)
    
    let lines = 
        System.IO.File.ReadLines( fileName )
        |> Seq.toArray

    let headLines, dataLines =
        lines
        |> Array.splitAt 6
     
    // -- read header
    let hd = headFromArray headLines
    
    // -- read height data
    let data =
        dataLines
        |> Array.map( fun x -> 
            x.Split( ' ' )
            |> Array.map( fun d -> 
                if d <> "" then
                    System.Double.Parse( d )
                else
                    hd.noData ) )

    use writeFile = 
        System.IO.File.CreateText( 
            (sprintf "%s_%d.obj" 
                (Path.GetFileNameWithoutExtension( fileName ))) nth )

    let minX = hd.centerX - hd.cellSize * float( hd.rows / 2 ) + shift.X
    let minY = hd.centerY - hd.cellSize * float( hd.cols / 2 ) + shift.Y

    let mutable countRow = 0
    let mutable countAll = 0

    writeFile.WriteLine( 
        sprintf "g _%s" (Path.GetFileNameWithoutExtension( fileName )) )

    // -- write vertices
    for j in 0 .. nth .. hd.rows-1 do
        countRow <- countRow + 1
        for i in 0 .. nth ..  hd.cols-1 do
            countAll <- countAll + 1
            writeFile.WriteLine( 
                sprintf "v %f %f %f" 
                    (scale * (minX + float( i ) * hd.cellSize ) )
                    (scale * (minY + float( hd.rows-1 - j ) * hd.cellSize ) )
                    (scale * data.[j].[i] ) )

    let to1D i j =
        let idx = 
            i * ( countRow  ) + j + 1 
        if idx > countAll then
            failwith (sprintf "Error IDX: %d %d -> %d" i j idx)
        idx

    // -- write faces
    for j in 0 .. countRow - 2 do
        for i in 0 .. countRow - 2 do
            writeFile.WriteLine(
                sprintf "f %d %d %d %d" 
                    (to1D j i)
                    (to1D j (i+1))
                    (to1D (j+1) (i+1))
                    (to1D (j+1) i) ) 


[<EntryPoint>]
let main argv =

    let nth   = optionValue argv "nth"  Int32.Parse 1
    let scale = optionValue argv "scl" Double.Parse 1.0
    let shift = optionValue argv "sft"   Vec2.Parse { X= 0.; Y= 0. }  
    let cut   = optionValue argv "cut" Bound2.Parse { min= { X= 0.; Y= 0. }; 
                                                      max= { X= 0.; Y= 0. } }
                                                    
    let success = onCurrentDirectoryFiles argv (esriToObj nth shift scale cut) 
    
    if not success then
        printfn "Convert ESRI Raster ASCII to OBJ"
        printfn "  Usage: "
        printfn "      ErsiToBJ [Option]* file"
        printfn "  Options:"
        printfn "      -cut: Rectangular cutting region in new coordinates - " 
        printfn "            after shift and scale., default: 0.,0.:0.,0 (disabled)"
        printfn "      -nth: Use every nth grid value, default: 1"
        printfn "      -scl: Scaling factor, default: 1."
        printfn "      -sft: XY shift vector added to coordinates, default: 0.,0."
        printfn "  Example:"
        printfn "      ErsiToOBJ -nth 5 -sft -5600.,-100432. -scl 0.01 -cut 156.0,234.4:513.0,1254. *.asc"
   
    0
