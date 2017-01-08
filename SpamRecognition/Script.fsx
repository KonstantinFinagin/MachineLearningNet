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

let casedTokenizer (text : string) =
    text
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

let casedTokens =
    training 
    |> Seq.map snd
    |> vocabulary casedTokenizer

let evaluate (tokenizer : Tokenizer) (tokens : Token Set) =
    let classifier = train training tokenizer tokens
    validation 
    |> Seq.averageBy (fun (docType, sms) -> if docType = classifier sms then 1.0 else 0.0)
    |> printfn "Correctly classified: %.3f"
        
// using top n words in spam and ham
let top n (tokenizer : Tokenizer) (docs : string[]) =
    let tokenized = docs |> Array.map tokenizer
    let tokens = tokenized |> Set.unionMany
    tokens 
    |> Seq.sortByDescending (fun t -> countIn tokenized t)
    |> Seq.take n
    |> Set.ofSeq

let ham, spam =
    let rawHam, rawSpam =
        training 
        |> Array.partition (fun (lbl,_) -> lbl = Ham)
    (rawHam |> Array.map snd, rawSpam |> Array.map snd)

let hamCount = ham |> vocabulary casedTokenizer |> Set.count
let spamCount = spam |> vocabulary casedTokenizer |> Set.count

let topHam = ham |> top (hamCount / 10) casedTokenizer
let topSpam = spam |> top (spamCount / 10) casedTokenizer

let topTokens = Set.union topHam topSpam

let commonTokens = Set.intersect topHam topSpam
let specificTokens = Set.difference topTokens commonTokens

evaluate wordTokenizer (["txt"] |> set)
evaluate wordTokenizer allTokens
evaluate casedTokenizer allTokens
evaluate casedTokenizer topTokens
evaluate casedTokenizer specificTokens