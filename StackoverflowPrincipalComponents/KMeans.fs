namespace Unsupervised

module KMeans =
    
    // pick random observations as centroids
    let pickFrom size k =
        
        // TODO rewrite uneffective function as it tends to repeat after a bad pick
        let rng = System.Random()
        let rec pick (set: int Set) =

            let candidate = rng.Next(size)
            let set = set.Add candidate
            if set.Count = k 
                then set 
                else pick set
        
        pick Set.empty |> Set.toArray

    let initialize observations k = 
        
        let size = Array.length observations

        let centroids = 
            pickFrom size k
            |> Array.mapi (fun i index -> i+1, observations.[index])

        let assignments = 
            observations
            |> Array.map (fun x -> 0,x)

        (assignments, centroids)

    let clusterize distance centroidOf observations k = 
        
        let rec search (assignments, centroids) = 

            // classify observation - assign observation to one of the centroids
            let classifier observation = 
                centroids
                |> Array.minBy (fun (_, centroid) -> distance observation centroid)
                |> fst

            // repick closest centroid for observations in assignments and form new ones
            let assignments' =
                assignments 
                |> Array.map (fun (_, observation) ->
                    let closestCentroidId = classifier observation
                    (closestCentroidId, observation))

            // detect whether asignments have been changed
            let changed = 
                (assignments, assignments')
                ||> Seq.zip
                |> Seq.exists (fun ((oldClusterId, _),(newClusterId, _)) -> not (oldClusterId = newClusterId))

            // get new centroids
            if changed 
            then
                let centroids' = 
                    assignments'
                    |> Seq.groupBy fst
                        // getting centroid for a group and reassigning it to a cluster
                    |> Seq.map (fun (clusterId, group) -> clusterId, (group |> Seq.map snd |> centroidOf))
                    |> Seq.toArray
                search (assignments', centroids')
            else centroids, classifier

        let initialValues = initialize observations k
        search initialValues