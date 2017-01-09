#I @"..\packages"
#r @"FSharp.Data.2.3.2\lib\net40\FSharp.Data.dll"

open FSharp.Data

// triple quotes ignore escape charachters and allow using quotes inside
type Questions = JsonProvider<"""https://api.stackexchange.com/2.2/questions?site=stackoverflow""">

let csQuestions = """https://api.stackexchange.com/2.2/questions?site=stackoverflow&tagged=C%23"""

Questions.Load(csQuestions).Items |> Seq.iter (fun q -> printfn "%s" q.Title)

// creating a type from local JSON sample
[<Literal>]
let sample = """
{
    "items":
    [
        {"tags":["java","arrays"], "owner": "a"},
        {"tags":["javascript","jquery"], "owner": "b"}
    ],
    "has_more" : true,
    "quota_max" : 300,
    "quota_remaining" : 299
 }
"""

type HardCodedQuestions = JsonProvider<sample>

let javaQuery = "https://api.stackexchange.com/2.2/questions?site=stackoverflow&tagged=java"
let javaQuestions = HardCodedQuestions.Load(javaQuery)
