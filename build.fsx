#r "paket:
version 5.257.0
framework: netstandard20
source https://api.nuget.org/v3/index.json
nuget Be.Vlaanderen.Basisregisters.Build.Pipeline 5.0.4 //"

#load "packages/Be.Vlaanderen.Basisregisters.Build.Pipeline/Content/build-generic.fsx"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open ``Build-generic``

let product = "Financial Converter"
let copyright = "Copyright (c) Cumps Consulting"
let company = "Cumps Consulting"

let assemblyVersionNumber = (sprintf "2.%s")
let nugetVersionNumber = (sprintf "2.%s")

let build = build assemblyVersionNumber
let setVersions = (setSolutionVersions assemblyVersionNumber product copyright company)
let test = testSolution
let publish = publish assemblyVersionNumber

supportedRuntimeIdentifiers <- [ "linux-x64"; "win-x64" ]

Target.create "Restore_Solution" (fun _ -> restore "FinancialConverter")

Target.create "Build_Solution" (fun _ ->
  setVersions "SolutionInfo.cs"
  build "FinancialConverter")

Target.create "Test_Solution" (fun _ -> test "FinancialConverter")

Target.create "Publish_Solution" (fun _ ->
  [
    "FinancialConverter"
  ] |> List.iter publish)

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
