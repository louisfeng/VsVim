#light

namespace Vim
open System.IO
open System.Collections.Generic
open System.ComponentModel.Composition
open System.Text

[<Export(typeof<IFileSystem>)>]
type internal FileSystem() =

    /// The environment variables considered when loading a .vimrc
    let _environmentVariables = ["%HOME%"; "%HOMEDRIVE%%HOMEPATH%"; "%VIM%"; "%USERPROFILE%"]

    let _fileNames = [".vsvimrc"; "_vsvimrc"; ".vimrc"; "_vimrc" ]

    /// Read all of the lines from the given StreamReader.  This will return whether or not 
    /// an exception occurred during processing and if not the lines that were read
    /// Read all of the lines from the file at the given path.  If this fails None
    /// will be returned
    member x.ReadAllLinesCore (streamReader : StreamReader) =
        let list = List<string>()
        let mutable line = streamReader.ReadLine()
        while line <> null do
            list.Add line
            line <- streamReader.ReadLine()

        list

    /// This will attempt to read the path using first the encoding dictated by the BOM and 
    /// if there is no BOM it will try UTF8.  If either encoding encounters errors trying to
    /// process the file then this function will also fail
    member x.ReadAllLinesBomAndUtf8 (path : string) = 
        let encoding = UTF8Encoding(false, true)
        use streamReader = new StreamReader(path, encoding, true)
        x.ReadAllLinesCore streamReader

    /// Read the lines with the Latin1 encoding.  
    member x.ReadAllLinesLatin1 (path : string) = 
        let encoding = Encoding.GetEncoding("Latin1")
        use streamReader = new StreamReader(path, encoding, false)
        x.ReadAllLinesCore streamReader

    /// Forced utf8 encoding
    member x.ReadAllLinesUtf8 (path : string) = 
        let encoding = Encoding.UTF8
        use streamReader = new StreamReader(path, encoding, false)
        x.ReadAllLinesCore streamReader

    /// Now we do the work to support various file encodings.  We prefer the following order
    /// of encodings
    ///
    ///  1. BOM 
    ///  2. UTF8
    ///  3. Latin1
    ///  4. Forced UTF8 and accept decoding errors
    ///
    /// Ideally we would precisely emulate vim here.  However replicating all of their encoding
    /// error detection logic and mixing it with the .Net encoders is quite a bit of work.  This
    /// pattern lets us get the vast majority of cases with a much smaller amount of work
    member x.ReadAllLinesWithEncoding (path: string) =
        let all = 
            [| 
                x.ReadAllLinesBomAndUtf8; 
                x.ReadAllLinesLatin1;
                x.ReadAllLinesUtf8;
            |]

        let mutable lines : List<string> option = None
        let mutable i = 0
        while i < all.Length && Option.isNone lines do
            try
                let current = all.[i]
                lines <- Some (current path)
            with
                | _ -> ()

            i <- i + 1

        lines

    /// Read all of the lines from the file at the given path.  If this fails None
    /// will be returned
    member x.ReadAllLines path =

        // Yes I realize I wrote an entire blog post on why File.Exists is an evil
        // API to use and I'm using it in this code.  In this particular case though
        // the use is OK because first and foremost we deal with the exceptions 
        // that can be thrown.  Secondly this is only used because it makes debugging
        // significantly easier as the exception thrown breaks. 
        //
        // Additionally I will likely be changing it to avoid the exception break
        // at a future time
        // 
        // http://blogs.msdn.com/b/jaredpar/archive/2009/12/10/the-file-system-is-unpredictable.aspx 
        if System.String.IsNullOrEmpty path then 
            None
        elif System.IO.File.Exists path then 

            match x.ReadAllLinesWithEncoding path with
            | None -> None
            | Some list -> list.ToArray() |> Some

        else
            None

    member x.GetVimRcDirectories() = 
        let getEnvVarValue var = 
            match System.Environment.ExpandEnvironmentVariables(var) with
            | var1 when System.String.Equals(var1,var,System.StringComparison.InvariantCultureIgnoreCase) -> None
            | null -> None
            | value -> Some(value)

        _environmentVariables
        |> Seq.map getEnvVarValue
        |> SeqUtil.filterToSome

    member x.GetVimRcFilePaths() =

        let standard = 
            x.GetVimRcDirectories()
            |> Seq.map (fun path -> _fileNames |> Seq.map (fun name -> Path.Combine(path,name)))
            |> Seq.concat

        // If the MYVIMRC environment variable is set then prefer that path over the standard
        // paths
        match SystemUtil.TryGetEnvironmentVariable "MYVIMRC" with
        | None -> standard
        | Some filePath -> Seq.append [ filePath ] standard

    member x.LoadVimRcContents () = 
        let readLines path = 
            match x.ReadAllLines path with
            | None -> None
            | Some lines -> 
                let contents = {
                    FilePath = path
                    Lines = lines
                } 
                Some contents
        x.GetVimRcFilePaths()  |> Seq.tryPick readLines

    interface IFileSystem with
        member x.EnvironmentVariables = _environmentVariables 
        member x.VimRcFileNames = _fileNames
        member x.GetVimRcDirectories () = x.GetVimRcDirectories()
        member x.GetVimRcFilePaths() = x.GetVimRcFilePaths()
        member x.LoadVimRcContents () = x.LoadVimRcContents()
        member x.ReadAllLines path = x.ReadAllLines path

