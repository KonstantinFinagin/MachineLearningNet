#I @"..\packages\"
#load "PCA.fs"   

#r @"MathNet.Numerics.3.17.0\lib\net40\MathNet.Numerics.dll"
#r @"MathNet.Numerics.FSharp.3.17.0\lib\net40\MathNet.Numerics.FSharp.dll"

open MathNet
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.Statistics

open System
open System.IO
open Unsupervised

let folder = __SOURCE_DIRECTORY__
let file = "userprofiles-toptags.txt"

let headers, observations = 

    let raw = 
        folder + "/" + file
        |> File.ReadAllLines

    let headers = (raw.[0].Split ',').[1..]

    let observations = 
        raw.[1..]
        |> Array.map (fun line -> (line.Split ',').[1..])
        |> Array.map (Array.map float)

    headers, observations

let correlations = 
    observations
    |> Matrix.Build.DenseOfColumnArrays
    |> Matrix.toRowArrays
    |> Correlation.PearsonMatrix

let feats = headers.Length
let correlated = 
    [
        for col in 0.. (feats - 1) do
            for row in (col+1) .. (feats-1) ->
                correlations.[col,row], headers.[col], headers.[row]
    ]
    |> Seq.sortBy (fun (corr, f1, f2) -> - abs corr)
    |> Seq.take 20
    |> Seq.iter (fun (corr, f1, f2) -> printfn "%s %s : %.2f" f1 f2 corr)