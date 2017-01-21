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

let graph =
    Chart.Combine [
        Chart.Line count
        Chart.Line (ma 7 count)
        Chart.Line (ma 30 count) ] 

let displayedGraph = graph.ShowChart();   
System.Windows.Forms.Application.Run(displayedGraph)
0



