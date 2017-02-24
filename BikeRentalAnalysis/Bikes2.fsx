﻿#I @"..\packages\"
#r @"FSharp.Data.2.3.2\lib\net40\FSharp.Data.dll"
#r @"FSharp.Charting.0.90.14\lib\net40\FSharp.Charting.dll"
#r "System.Windows.Forms.DataVisualization.dll"
#r @"MathNet.Numerics.Signed.3.17.0\lib\net40\MathNet.Numerics.dll"
#r @"MathNet.Numerics.FSharp.Signed.3.17.0\lib\net40\MathNet.Numerics.FSharp.dll"

open FSharp.Data
open FSharp.Charting
open System
open System.Drawing
open System.Windows.Forms
open System.Windows.Forms.DataVisualization
open MathNet
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double

[<Literal>]
let path = __SOURCE_DIRECTORY__ + @"\..\Data\" + "day.csv"

type Data = CsvProvider<path>
let dataSet = Data.Load(path)
let data = dataSet.Rows

type Vec = Vector<float>
type Mat = Matrix<float>

// moving average
let ma n (series : float seq) = 
    series
    |> Seq.windowed n
    |> Seq.map (fun xs -> xs |> Seq.average)
    |> Seq.toList

let count = seq { for obs in data -> float obs.Cnt } |> Seq.toList

// introducing linear regression model
type Obs = Data.Row
type Model = Obs -> float
type Featurizer = Obs -> float list

// computing the overall cost of two models
let cost (theta : Vec) (Y : Vec) (X : Mat) = 
    let ps  = Y - (theta * X.Transpose())
    ps * ps |> sqrt

let predict (theta : Vec) (v : Vec) = theta * v

let X = matrix [ for obs in data -> [1. ; float obs.Instant] ]
let Y = vector [ for obs in data -> float obs.Cnt ]

let theta = vector [6000.; -4.5]

predict theta (X.Row(0))
cost theta Y X    

let estimate (Y:Vec) (X:Mat) = 
    (X.Transpose() * X).Inverse() * X.Transpose() * Y

let result Y X = 
    let estimation = estimate Y X
    (estimation.[0], estimation.[1])

let seed = 314159
let rng = System.Random(seed)

// Evolving and validating models rapidly
let shuffle (arr: 'a[]) = 
    let arr = Array.copy arr
    let l = arr.Length

    for i in (l-1) .. -1 .. 1 do
        let temp = arr.[i]
        let j = rng.Next(0, i+1)
        arr.[i] <- arr.[j]
        arr.[j] <- temp
    arr

let training, validation = 
    let shuffled =
        data
        |> Seq.toArray
        |> shuffle

    let size = 
        0.7 * float (Array.length shuffled) |> int
    shuffled.[..size],
    shuffled.[size+1..]

let predictor (f : Featurizer) (theta : Vec) = 
    f >> vector >> (*) theta

let evaluate (model:Model) (data:Obs seq) = 
    data
    |> Seq.averageBy (fun obs -> abs(model obs - float obs.Cnt))

let model (f: Featurizer) (data: Obs seq) = 
    let Yt, Xt = 
        data
        |> Seq.toList
        |> List.map (fun obs -> float obs.Cnt, f obs)
        |> List.unzip
    let theta = estimate (vector Yt) (matrix Xt)
    let predict = predictor f theta
    theta, predict

let featurizer0 (obs:Obs) = 
    [ 1.; float obs.Instant ]

let (theta0, model0) = model featurizer0 training

//----------------------------------------------------------
let graph =
    Chart.Combine [
        Chart.Line (ma 7 count)
        // batched_error 0.000001

        ] 

let displayedGraph = graph.ShowChart();   
System.Windows.Forms.Application.Run(displayedGraph)
0

