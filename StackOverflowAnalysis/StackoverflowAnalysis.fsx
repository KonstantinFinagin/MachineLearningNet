#I @"..\packages"
#r @"FSharp.Data.2.4.6\lib\net40\FSharp.Data.dll"

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

let questionQuery = """https://api.stackexchange.com/2.2/questions?site=stackoverflow"""

// Implementing simple DSL for queries

let tagged tags query =
    // join the tags in a ; separated string
    let joinedTags = tags |> String.concat ";"
    sprintf "%s&tagged=%s" query joinedTags

let page p query = sprintf "%s&page=%i" query p

let pageSize s query = sprintf "%s&pagesize=%i" query s

let extractQuestions (query : string) = Questions.Load(query).Items

let ``C#`` = "C%23"
let ``F#`` = "F%23"

let fsSample = 
    questionQuery
    |> tagged [``F#``]
    |> pageSize 100
    |> extractQuestions

let csSample =
    questionQuery
    |> tagged [``C#``]
    |> pageSize 100
    |> extractQuestions

let analyzeTags (qs:Questions.Item seq) =
    qs
    |> Seq.collect (fun question -> question.Tags)
    |> Seq.countBy id
    |> Seq.filter (fun (_, count) -> count > 2)
    |> Seq.sortBy (fun (_, count) -> -count)
    |> Seq.iter (fun (tag, count) -> printfn "%s, %i" tag count)

// analyzeTags fsSample
// analyzeTags csSample

// World bank data provider