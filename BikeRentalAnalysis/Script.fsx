#I @"..\packages\"
#r @"FSharp.Data.2.3.2\lib\net40\FSharp.Data.dll"
#r @"FSharp.Charting.0.90.14\lib\net40\FSharp.Charting.dll"

open FSharp.Data
open FSharp.Charting

[<Literal>]
let path = __SOURCE_DIRECTORY__ + @"\..\Data\" + "day.csv"

type Data = CsvProvider<path>
let dataSet = Data.Load(path)
let data = dataSet.Rows

let all = Chart.Line [ for obs in data -> obs.Cnt ]

