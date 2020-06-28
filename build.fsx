#r "paket:
version 5.247.2
framework: netstandard20
source https://api.nuget.org/v3/index.json
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 4.2.1 //"

#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open ``Build-generic``

let product = "CODA to Gripp ING"
let copyright = "Copyright (c) Cumps Consulting"
let company = "Cumps Consulting"

let dockerRepository = "coda-to-gripp-ing"
let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "2.%s")

let build = buildSolution assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publish = publish assemblyVersionNumber

supportedRuntimeIdentifiers <- [ "linux-x64" ]

Target.create "Restore_Solution" (fun _ -> restore "CodaToGrippIng")

Target.create "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  build "CodaToGrippIng")

Target.create "Test_Solution" (fun _ -> test "CodaToGrippIng")

Target.create "Publish_Solution" (fun _ ->
  [
    "CodaToGrippIng"
  ] |> List.iter publish


  )

Target.create "Build" ignore
Target.create "Test" ignore
Target.create "Publish" ignore

"NpmInstall"
  ==> "DotNetCli"
  ==> "Clean"
  ==> "Restore_Solution"
  ==> "Build_Solution"
  ==> "Build"

"Build"
  ==> "Test_Solution"
  ==> "Test"

"Test"
  ==> "Publish_Solution"
  ==> "Publish"

// By default we build & test
Target.runOrDefault "Test"
