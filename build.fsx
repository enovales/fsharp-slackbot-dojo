// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.UserInputHelper
open System
open System.IO

// The name of the project
// (used by directory in 'src')
let project = "fsharp-slackbot-dojo"

// Short summary of the project
// (used as description in AssemblyInfo)
let summary = "Slack integration written with F#"

// File system information
let solutionFile  = "fsharp-slackbot-dojo.sln"

// Default target configuration
let configuration = "Debug"

// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|Shproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | f when f.EndsWith("shproj") -> Shproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)

// Copies binaries from default VS location to expected bin folder
// But keeps a subdirectory structure for each project in the
// src folder to support multiple project outputs
Target "CopyBinaries" (fun () ->
    ensureDirectory "bin"

    !! "src/**/*.??proj"
    -- "src/**/*.shproj"
    |>  Seq.map (fun f -> ((System.IO.Path.GetDirectoryName f) </> "bin" </> configuration, "bin" </> (System.IO.Path.GetFileNameWithoutExtension f)))
    |>  Seq.iter (fun (fromDir, toDir) -> CopyDir toDir fromDir (fun _ -> true))
)

// --------------------------------------------------------------------------------------
// Clean build results

let vsProjProps = 
#if MONO
    [ ("DefineConstants","MONO"); ("Configuration", configuration) ]
#else
    [ ("Configuration", configuration); ("Platform", "Any CPU") ]
#endif

Target "Clean" (fun () ->
    !! solutionFile |> MSBuildReleaseExt "" vsProjProps "Clean" |> ignore
    CleanDirs ["bin"; "temp"; "docs/output"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "All" DoNothing

Target "BuildBinaries" (fun () ->
    let res = 
      !! solutionFile
      |> MSBuildReleaseExt "" vsProjProps "Rebuild"
      
    trace(String.Join("\n", res))
    ()
)

Target "Build" (fun () -> ())

"BuildBinaries"
  ==> "Build"
"CopyBinaries"
  ==> "Build"  

RunTargetOrDefault "All"
