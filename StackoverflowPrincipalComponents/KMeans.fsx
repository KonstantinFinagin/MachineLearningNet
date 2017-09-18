    #I @"..\packages\"
    #r @"FSharp.Charting.0.90.14\lib\net40\FSharp.Charting.dll"
    #load "KMeans.fs"    
    
    open System
    open System.IO
    open FSharp.Charting
    open Unsupervised.KMeans

    let folder = __SOURCE_DIRECTORY__
    let file = "userprofiles-toptags.txt"

    let headers, observations = 

        let raw = 
            folder + "/" + file
            |> File.ReadAllLines

        let headers = (raw.[0].Split ',').[1..]

        let observations = 
            raw.[1..]
            |> Array.map (fun line -> (line.Split ',').[1..])
            |> Array.map (Array.map float)

        headers, observations

    printfn "%16s %8s %8s %8s" "Tag Name" "Avg" "Min" "Max"
    headers 
    |> Array.iteri (fun i name -> 
        let col = observations |> Array.map (fun obs -> obs.[i])
        let avg = col |> Array.average
        let min = col |> Array.min
        let max = col |> Array.max
        printfn "%16s %8.1f %8.1f %8.1f" name avg min max)

    // using KMeans clustering
    type Observation = float[]
    let numberOfFeatures = headers.Length

    // euclidean distance
    let distance (obs1:Observation) (obs2:Observation) = 
        (obs1, obs2)
        ||> Seq.map2 (fun u1 u2 -> pown (u1-u2) 2)
        |> Seq.sum
        |> sqrt

    let centroidOf (cluster:Observation seq) =
        Array.init numberOfFeatures (fun f -> cluster |> Seq.averageBy(fun user -> user.[f]))

    let observations1 = 
        observations
        |> Array.map (Array.map float)
        |> Array.filter (fun x -> Array.sum x > 0.)

    let (clusters1, classifier1) = 
        let clustering = clusterize distance centroidOf
        let k = 5
        clustering observations1 k
    
        
    clusters1
    |> Seq.iter (fun (id, profile) -> 
        printfn "CLUSTER %i" id
        profile |> Array.iteri (fun i value -> printfn "%16s %.1f" headers.[i] value)
    )

    let labels = ChartTypes.LabelStyle(Interval = 0.25)
    
    let chart1 = 
        headers
        |> Seq.mapi (fun i name -> 
            name, 
            observations |> Seq.averageBy (fun obs -> obs.[i])
            )
        |> Seq.sortBy(fun (name, value) -> value)
        |> Chart.Bar 

    let chart2 =
        [ for (id, profile) in clusters1 ->
            profile
            |> Seq.mapi (fun i value -> headers.[i], value)
            |> Chart.Bar  
        ]

    let printClusters (observations, classifier) =
        observations
        |> Seq.countBy (fun obs -> classifier obs)
        |> Seq.iter (fun (clusterId, count) -> printfn "Cluster %i: %i elements" clusterId count)

    (observations1, classifier1) |> printClusters

    // adding row noramizations for the same scale of tags usage (percentage)
    let rowNormalizer (obs:Observation) : Observation = 
        let max = obs |> Seq.max
        obs |> Array.map (fun tagUse -> tagUse / max)

    let observations2 = 
        observations 
        |> Array.filter (fun x -> Array.sum x > 0.)
        |> Array.map (Array.map float)
        |> Array.map rowNormalizer

    let (clusters2, classifier2) = 
        let clustering = clusterize distance centroidOf
        let k = 5
        clustering observations2 k

    (observations2, classifier2) |> printClusters

    let chart3 = 
        [ for (id, profile) in clusters2 ->
            profile
            |> Seq.mapi (fun i value -> headers.[i], value)
            |> Chart.Bar  
        ]

    // identifying the best number of clusters 
    let ruleOfThumb (n : int) = sqrt (float n/2.)
    let k_ruleOfThumb = ruleOfThumb (observations2.Length)

    // Akaike information criterion
    let squareError (obs1:Observation) (obs2:Observation) = 
        (obs1,obs2)
        ||> Seq.zip
        |> Seq.sumBy (fun (x1,x2) -> pown (x1-x2) 2)

    // residual sum of squares
    let RSS (dataset:Observation[]) centroids =
        dataset 
        |> Seq.sumBy (fun obs -> centroids |> Seq.map (squareError obs) |> Seq.min)

    let AIC (dataset:Observation[]) centroids =
        let k = centroids |> Seq.length
        let m = dataset.[0] |> Seq.length
        RSS dataset centroids + float (2 * m * k)

    // long-running application of AIC
    
//    let pickChart = 
//        [1..25]
//        |> Seq.map (fun k -> 
//            let value = 
//                printfn "%i" k
//
//                [ for j in 1..10 -> // running for several times to eliminate flukes due to improper initial values selection 
//                    let (clusters, classifier) = 
//                        printfn "%i" j
//                        let clustering = clusterize distance centroidOf
//                        clustering observations2 k
//                    AIC observations2 (clusters |> Seq.map snd)]
//                |> List.average
//            k, value)
//        |> Chart.Line
    
    let (bestClusters, bestClassifier) =    
        let clustering = clusterize distance centroidOf
        let k = 10
        seq {
            for _ in 1 .. 20 -> clustering observations2 k    
        }
        |> Seq.minBy (fun (cs, f) -> RSS observations2 (cs |> Seq.map snd))

    bestClusters 
    |> Seq.iter (fun (id, profile) -> 
        printfn "CLUSTER %i" id
        profile 
        |> Array.iteri (fun i value -> if value > 0.2 then printfn "%16s %.1f" headers.[i] value))


    let graph = Chart.Combine(chart3) |> Chart.WithXAxis(LabelStyle=labels)

    let displayedGraph = graph.ShowChart();   



