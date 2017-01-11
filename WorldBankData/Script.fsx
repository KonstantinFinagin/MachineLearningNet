#I @"..\packages"
#r @"FSharp.Data.2.3.2\lib\net40\FSharp.Data.dll"
#r @"R.NET.Community.1.6.5\lib\net40\RDotNet.dll"
#r @"RProvider.1.1.20\lib\net40\RProvider.Runtime.dll"
#r @"RProvider.1.1.20\lib\net40\RProvider.dll"
#r @"Deedle.1.2.5\lib\net40\Deedle.dll"
#r @"Deedle.RPlugin.1.2.5\lib\net40\Deedle.RProvider.Plugin.dll"

open FSharp.Data
open RProvider
open RProvider.``base``
open RProvider.graphics
open Deedle
open Deedle.RPlugin

let wb = WorldBankData.GetDataContext ()
wb.Countries.Japan.CapitalCity

let countries = wb.Countries

let pop2000 = [ for c in countries -> c.Indicators.``Population, total``.[2000]]
let pop2010 = [ for c in countries -> c.Indicators.``Population, total``.[2010]]

let surface = [for c in countries -> c.Indicators.``Surface area (sq. km)``.[2010]]

R.summary(surface) |> R.print
R.hist(surface |> R.log)

R.plot(surface, pop2010)

let pollution = [ for c in countries -> c.Indicators.``CO2 emissions (kt)``.[2000]]
let education = [ for c in countries -> c.Indicators.``School enrollment, secondary, male (% gross)``.[2000]]

let rdf = 
    [   "Pop2000", box pop2000
        "Pop2010", box pop2010
        "Surface", box surface
        "Pollution", box pollution
        "Education", box education
    ]
    |> namedParams
    |> R.data_frame    

// scatterplot of all features
rdf |> R.plot

// summary of all features
rdf |> R.summary |> R.print

// Deedle test
let series1 = series [ "Alpha", 1.; "Bravo", 2.; "Delta", 4. ]
let series2 = series [ "Bravo", 20.; "Charlie", 30.; "Delta", 40. ]
let toyFrame = frame [ "First", series1; "Second", series2 ]

series1 |> Stats.sum
toyFrame |> Stats.mean
toyFrame?Second |> Stats.mean

// dynamic properties make sense for deedle to add data to existing frame
toyFrame?New <- toyFrame?First + toyFrame?Second
toyFrame |> Stats.mean





