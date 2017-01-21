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
type Observation = Data.Row

let model (theta0, theta1) (obs : Observation) = theta0 + theta1 * (float obs.Instant)

let model0 = model(4504., 0.)
let model1 = model(6000., -4.5)

type Model = Observation -> float

// computing the overall cost of two models
let cost (data : Observation seq) (m : Model) =
    data 
    |> Seq.sumBy (fun x -> pown (float x.Cnt - m x) 2)
    |> sqrt

let graph =
    Chart.Combine [
        Chart.Line count
        Chart.Line [ for obs in data -> model0 obs ]
        Chart.Line [ for obs in data -> model1 obs ]
        //Chart.Line (ma 7 count)
        //Chart.Line (ma 30 count) 
        ] 

let displayedGraph = graph.ShowChart();   
System.Windows.Forms.Application.Run(displayedGraph)
0



