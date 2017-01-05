open System.IO

type Observation = { Label:string; Pixels:int[] }

let toObservation (csvData:string) = 
    let columns = csvData.Split(',')
    let label = columns.[0]
    let pixels = columns.[1..] |> Array.map int
    { Label = label; Pixels = pixels }

let reader path = 
    let data = File.ReadAllLines path
    data.[1..] 
    |> Array.map toObservation

let trainingPath = @"D:\Learning\MachineLearningNet\DigitRecognition\Data\trainingSample.csv"
let trainingData = reader trainingPath

let manhattanDistance (pixels1, pixels2) = 
    Array.zip pixels1 pixels2
    |> Array.map (fun (x,y) -> abs (x-y))
    |> Array.sum

let euclideanDistance (pixels1, pixels2) = 
    Array.zip pixels1 pixels2
    |> Array.map (fun (x, y) -> pown (x-y) 2)
    |> Array.sum
    |> double
    |> sqrt

let train (trainingSet : Observation[]) distance =
    let classify (pixels : int[]) =
        trainingSet
        |> Array.minBy (fun x -> distance (x.Pixels, pixels))
        |> fun x -> x.Label
    classify

let manhattanClassifier = train trainingData manhattanDistance
let euclideanClassifier = train trainingData euclideanDistance

let validationPath = @"D:\Learning\MachineLearningNet\DigitRecognition\Data\validationsample.csv"
let validationData = reader validationPath

validationData 
|> Array.averageBy (fun x -> if manhattanClassifier x.Pixels = x.Label then 1. else 0.)
|> printfn "Correct: %.3f"