#load "NaiveBayes.fs"

open System.IO
open NaiveBayes.Classifier
open System.Text.RegularExpressions

type DocType = 
    | Ham
    | Spam 

let parseDocType (label:string) = 
    match label with
    | "ham" -> Ham
    | "spam" -> Spam
    | _ -> failwith "Unknown label"
   
let parseLine (line:string) = 
    let split = line.Split('\t')
    let label = split.[0] |> parseDocType
    let message = split.[1]
    (label, message)

let fileName = "SMSSpamCollection"
let path = __SOURCE_DIRECTORY__ + @"..\..\Data\" + fileName

let dataSet = 
    File.ReadAllLines path
    |> Array.map parseLine

// use first 1000 for training and the rest for classification
let training = 
    dataSet |> Array.take 1000

let validation = 
    dataSet |> Array.skip 1000

let matchWords = Regex(@"\w+")

// tokenizing a line of text with regular expressions
let wordTokenizer (text : string) = 
    text.ToLowerInvariant()
    |> matchWords.Matches
    |> Seq.cast<Match>
    |> Seq.map (fun m -> m.Value)
    |> Set.ofSeq

let vocabulary (tokenizer : Tokenizer) (corpus : string seq) =
    corpus
    |> Seq.map tokenizer
    |> Set.unionMany

let allTokens = 
    training 
    |> Seq.map snd
    |> vocabulary wordTokenizer

let txtClassifier = train training wordTokenizer allTokens

validation 
|> Seq.averageBy (fun (docType, sms) -> 
    if docType = txtClassifier sms then 1.0 else 0.0)
|> printfn "Based on 'txt', correcly classified: %.3f"
