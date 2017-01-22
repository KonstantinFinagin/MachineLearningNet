#I @"..\packages\"
#r @"FSharp.Data.2.3.2\lib\net40\FSharp.Data.dll"
#r @"FSharp.Charting.0.90.14\lib\net40\FSharp.Charting.dll"
#r "System.Windows.Forms.DataVisualization.dll"

open FSharp.Data
open FSharp.Charting
open System
open System.Drawing
open System.Windows.Forms
open System.Windows.Forms.DataVisualization

[<Literal>]
let path = __SOURCE_DIRECTORY__ + @"\..\Data\" + "day.csv"

type Data = CsvProvider<path>
let dataSet = Data.Load(path)
let data = dataSet.Rows

// moveing average
let ma n (series : float seq) = 
    series
    |> Seq.windowed n
    |> Seq.map (fun xs -> xs |> Seq.average)
    |> Seq.toList

let count = seq { for obs in data -> float obs.Cnt } |> Seq.toList

// introducing linear regression model
type Obs = Data.Row

let model (theta0, theta1) (obs : Obs) = theta0 + theta1 * (float obs.Instant)

let model0 = model(4504., 0.)
let model1 = model(6000., -4.5)

type Model = Obs -> float

// computing the overall cost of two models
let cost (data : Obs seq) (m : Model) =
    data 
    |> Seq.sumBy (fun x -> pown (float x.Cnt - m x) 2)
    |> sqrt

let overallCost = cost data

// introducing gradient descent

// update function to modify theta
let update alpha (theta0, theta1) (obs : Obs) =
    let y = float obs.Cnt
    let x = float obs.Instant   
    let theta0' = theta0 - 2. * alpha * 1. * (theta0 + theta1 * x - y)
    let theta1' = theta1 - 2. * alpha * x * (theta0 + theta1 * x - y)
    theta0', theta1'

let obs100 = data |> Seq.item 100
let testUpdate = update 0.00001 (0.0,0.0) obs100
cost [obs100] (model (0.0,0.0))
cost [obs100] (model testUpdate)

let stochastic rate (theta0, theta1) =
    data
    |> Seq.fold (fun (t0, t1) obs -> 
        printfn "%.4f, %.4f" t0 t1
        update rate (t0, t1) obs) (theta0, theta1)

(*
// 1e-8 is the best rate
let tune_rate =
    [ for r in 1 .. 20 -> (pown 0.1 r), stochastic (pown 0.1 r) (0.0,0.0) |> model |> overallCost ]
    
    *)

let rate = pown 0.1 8

let model2 = model (stochastic rate (0.0,0.0))

let hiRate = 10.0 * rate

let error_eval = 
    data
    |> Seq.scan (fun (t0,t1) obs -> update hiRate (t0,t1) obs) (0.0,0.0)
    |> Seq.map (model >> overallCost)
    |> Chart.Line


// batch update for the gradient descent
let batchUpdate rate (theta0, theta1) (data : Obs seq) =
    let updates = 
        data
        |> Seq.map (update rate (theta0, theta1))
    let theta0' = updates |> Seq.averageBy fst
    let theta1' = updates |> Seq.averageBy snd
    theta0', theta1'

let batch rate iters = 
    let rec search (t0, t1) i =
        if i=0 then (t0,t1)
        else
            search (batchUpdate rate (t0,t1) data) (i-1)
    search (0.0,0.0) iters

let batched_error rate =
    Seq.unfold (fun (t0, t1) -> 
        let (t0', t1') = batchUpdate rate (t0,t1) data
        let err = model (t0, t1) |> overallCost
        Some(err, (t0', t1'))) (0.0,0.0)
    |> Seq.take 100
    |> Seq.toList
    |> Chart.Line   

let graph =
    Chart.Combine [
        //Chart.Line count
        //Chart.Line [ for obs in data -> model2 obs]
        error_eval
        batched_error 0.000001
        //Chart.Line [ for obs in data -> model0 obs ]
        //Chart.Line [ for obs in data -> model1 obs ]
        //Chart.Line (ma 7 count)
        //Chart.Line (ma 30 count) 
        ] 

let displayedGraph = graph.ShowChart();   
System.Windows.Forms.Application.Run(displayedGraph)
0



