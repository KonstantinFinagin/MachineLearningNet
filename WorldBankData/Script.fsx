#I @"..\packages"
#r @"FSharp.Data.2.3.2\lib\net40\FSharp.Data.dll"
#r @"R.NET.Community.1.6.5\lib\net40\RDotNet.dll"
#r @"RProvider.1.1.20\lib\net40\RProvider.Runtime.dll"
#r @"RProvider.1.1.20\lib\net40\RProvider.dll"

open FSharp.Data
open RProvider
open RProvider.``base``
open RProvider.graphics

let wb = WorldBankData.GetDataContext ()
wb.Countries.Japan.CapitalCity

let countries = wb.Countries

let pop2000 = [ for c in countries -> c.Indicators.``Population, total``.[2000]]
let pop2010 = [ for c in countries -> c.Indicators.``Population, total``.[2010]]

let surface = [for c in countries -> c.Indicators.``Surface area (sq. km)``.[2010]]

R.summary(surface) |> R.print