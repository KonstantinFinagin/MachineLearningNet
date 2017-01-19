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

let topHam = ham |> top (hamCount / 5) casedTokenizer
let topSpam = spam |> top (spamCount / 20) casedTokenizer

let topTokens = Set.union topHam topSpam

let commonTokens = Set.intersect topHam topSpam
let specificTokens = Set.difference topTokens commonTokens

// using rare tokens to see what is present in ham and spam to enhance recognition

let rareTokens n (tokenizer : Tokenizer) (docs : string[]) =
    let tokenized = docs |> Array.map tokenizer
    let tokens = tokenized |> Set.unionMany
    tokens
    |> Seq.sortBy (fun t -> countIn tokenized t)
    |> Seq.take n
    |> Set.ofSeq

let rareHam = ham |> rareTokens 50 casedTokenizer
let rareSpam = spam |> rareTokens 50 casedTokenizer

// substitute phone numbers and codes

let phoneWords = Regex(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$")
let phone (text : string) = 
    match (phoneWords.IsMatch text) with
    | true -> "__PHONE__"
    | false -> text

let txtCode = Regex(@"\b\d{5}\b")
let txt (text : string) =
    match (txtCode.IsMatch text) with
    | true -> "__TXT__"
    | false -> text

// function composition
let smartTokenizer = casedTokenizer >> Set.map phone >> Set.map txt

let smartTokens = 
    specificTokens
    |> Set.add "__TXT__"
    |> Set.add "__PHONE__"

(*
evaluate wordTokenizer (["txt"] |> set)
evaluate wordTokenizer allTokens
evaluate casedTokenizer allTokens
evaluate casedTokenizer topTokens
evaluate casedTokenizer specificTokens
*)

evaluate smartTokenizer smartTokens

// spam probability based on length analysis

let lengthAnalysis len = 
    let long (msg : string) = msg.Length > len

    let ham, spam = 
        dataSet 
        |> Array.partition (fun (docType, _) -> docType = Ham)
        
    let countLong dataset =
        dataset  
        |> Array.filter (fun (_, sms) -> long sms)
        |> Array.length

    let spamAndLongCount = countLong spam
    let longCount = countLong dataSet

    let pSpam = (float spam.Length) / (float dataSet.Length)
    let pLongIfSpam = (float spamAndLongCount) / (float spam.Length)
    let pLong = (float longCount) / (float dataSet.Length)

    // applying Bayes theorem
    let pSpamIfLong = pLongIfSpam * pSpam / pLong

    pSpamIfLong

for l in 10 .. 10 .. 160 do
    printfn "P(Spam if Length > %i) = %.4f" l (lengthAnalysis l)

// errors in classification

let bestClassifier = train training smartTokenizer smartTokens

// check false positive error - when Ham is (in)correcly classified
validation 
|> Seq.filter (fun (docType, _) -> docType = Ham)
|> Seq.averageBy (fun (docType, sms) -> if docType = bestClassifier sms then 1.0 else 0.0)
|> printfn "Properly classified Ham: %.5f"

// check false negative error - when Spam is (in)correcly classified
validation 
|> Seq.filter (fun (docType, _) -> docType = Spam)
|> Seq.averageBy (fun (docType, sms) -> if docType = bestClassifier sms then 1.0 else 0.0)
|> printfn "Properly classified Spam: %.5f"
