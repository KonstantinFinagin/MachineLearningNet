#r @"..\packages\FSharp.Data.2.4.6\lib\net45\FSharp.Data.dll"

open FSharp.Data

[<Literal>]
let path = __SOURCE_DIRECTORY__ + @"\..\Data\" + "titanic.csv"
type Titanic = CsvProvider<path>

type Passenger = Titanic.Row

let dataset = Titanic.GetSample ()

dataset.Rows
    |> Seq.countBy(fun passenger -> passenger.Survived)
    |> Seq.iter(printfn "%A")

dataset.Rows
    |> Seq.averageBy(fun passenger -> if passenger.Survived then 1.0 else 0.0)
    |> printfn "Chances of survival: %.3f"

let survivalRate (passengers: Passenger seq) = 
    let total = passengers |> Seq.length
    let survivors = passengers |> Seq.filter(fun p -> p.Survived) |> Seq.length
    100.0 * (float survivors / float total)

let bySex = 
    dataset.Rows |> Seq.groupBy(fun p -> p.Sex)

bySex |> Seq.iter (fun (s,g) -> printfn "Sex %A: %f" s (survivalRate g))

let byClass = 
    dataset.Rows |> Seq.groupBy(fun p -> p.Pclass)

byClass |> Seq.iter (fun (s,g) -> printfn "Class %A: %f" s (survivalRate g))











