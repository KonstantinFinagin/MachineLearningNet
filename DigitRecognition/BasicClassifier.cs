namespace DigitRecognition
{
    using System;
    using System.Collections.Generic;

    public class BasicClassifier : IClassifier
    {
        private readonly IDistance distance;
        private IEnumerable<Observation> data;

        public BasicClassifier(IDistance distance)
        {
            this.distance = distance;
        }

        public void Train(IEnumerable<Observation> trainingSet)
        {
            this.data = trainingSet;
        }   

        public string Predict(int[] pixels)
        {
            Observation currentBest = null;

            var shortest = double.MaxValue;

            foreach (var observation in this.data)
            {
                var dist = this.distance.Between(observation.Pixels, pixels);
                if (!(dist < shortest))
                {
                    continue;
                }

                shortest = dist;
                currentBest = observation;
            }

            return currentBest?.Label;
        }
    }
}
