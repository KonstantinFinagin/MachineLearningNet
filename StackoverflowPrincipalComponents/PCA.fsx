#I @"..\packages\"

#r @"MathNet.Numerics.3.20.0\lib\net40\MathNet.Numerics.dll"
#r @"MathNet.Numerics.FSharp.3.20.0\lib\net40\MathNet.Numerics.FSharp.dll"
#r @"FSharp.Charting.0.90.14\lib\net40\FSharp.Charting.dll"
#r "System.Windows.Forms.DataVisualization.dll"

#load "PCA.fs"   

open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.Statistics

open System
open System.IO
open FSharp.Charting

open Unsupervised.PCA
open MathNet.Numerics.Statistics.Statistics
open FSharp.Charting
open System.Windows.Forms.DataVisualization.Charting

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

let normalized = normalize (headers.Length) observations
let (eValues, eVectors), projector = pca normalized

let total = eValues |> Seq.sumBy(fun x -> x.Magnitude)
eValues
|> Vector.toList
|> List.rev
|> List.scan (fun (_,cumul) value ->
    let percent = 100. * value.Magnitude / total
    let cumul = cumul + percent
    (percent, cumul)) (0.,0.)
|> List.tail
|> List.iteri (fun i (p,c) -> printfn "Feat %2i: %.2f%% (%.2f%%)" i p c)

let principalComponent comp1 comp2 =
    let features = headers.Length
    let title = sprintf "Component %i vs %i (%s vs %s)" comp1 comp2 headers.[comp1] headers.[comp2]
    let coords = Seq.zip (eVectors.Column(features-comp1)) (eVectors.Column(features-comp2))

    Chart.Point (coords, Title = title, Labels = headers, MarkerSize = 7)
        |> Chart.WithXAxis(
            Min = -1.0, 
            Max = 1.0,
            MajorGrid = ChartTypes.Grid(Interval = 0.25),
            LabelStyle= ChartTypes.LabelStyle(Interval = 0.25),
            MajorTickMark = ChartTypes.TickMark(Enabled = false))
        |> Chart.WithYAxis(  
            Min = -1.0, 
            Max = 1.0,
            MajorGrid = ChartTypes.Grid(Interval = 0.25),
            LabelStyle= ChartTypes.LabelStyle(Interval = 0.25),
            MajorTickMark = ChartTypes.TickMark(Enabled = false))

let projections comp1 comp2 = 
    let features = headers.Length
    let title = sprintf "Component %i vs %i (%s vs %s)" comp1 comp2 headers.[comp1] headers.[comp2]

    let coords =
        normalized
        |> Seq.map projector
        |> Seq.map(fun obs -> obs.[features-comp1], obs.[features-comp2])

    Chart.Point (coords, Title = title, MarkerSize = 2)
        |> Chart.WithXAxis(
            Min = -100.0, 
            Max = 100.0,
            MajorGrid = ChartTypes.Grid(Interval = 100.),
            LabelStyle= ChartTypes.LabelStyle(Interval = 100.),
            MajorTickMark = ChartTypes.TickMark(Enabled = false))
        |> Chart.WithYAxis(  
            Min = -100.0, 
            Max = 100.0,
            MajorGrid = ChartTypes.Grid(Interval = 100.),
            LabelStyle= ChartTypes.LabelStyle(Interval = 100.),
            MajorTickMark = ChartTypes.TickMark(Enabled = false))

let displayedGraph = principalComponent 7 6
let displayedGraph2 = projections 7 6

displayedGraph.ShowChart()
displayedGraph2.ShowChart()
0
