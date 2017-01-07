#load "NaiveBayes.fs"

open NaiveBayes.Classifier
open System.Text.RegularExpressions

let matchWords = Regex(@"\w+")

// tokenizing a line of text with regular expressions
let tokens (text : string) = 
    text.ToLowerInvariant()
    |> matchWords.Matches
    |> Seq.cast<Match>
    |> Seq.map (fun m -> m.Value)
    |> Set.ofSeq

