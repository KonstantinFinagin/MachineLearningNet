#I @"..\packages\"
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

type Model = Obs -> float

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

let graph =
    Chart.Combine [
        Chart.Line [ for obs in data -> model (result Y X) obs]
        Chart.Line (ma 7 count)
        // batched_error 0.000001

        ] 

let displayedGraph = graph.ShowChart();   
System.Windows.Forms.Application.Run(displayedGraph)
0

