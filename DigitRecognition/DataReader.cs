namespace DigitRecognition
{
    using System;
    using System.Linq;

    public class DataReader
    {
        private static Observation ObservationFactory(string data)
        {
            var commaSeparated = data.Split(',');

            var label = commaSeparated[0];

            var pixels = commaSeparated
                .Skip(1)
                .Select(x => Convert.ToInt32(x))
                .ToArray();

            return new Observation(label, pixels);
        }

        public static Observation[] ReadObservations(string dataPath)
        {
            
        }
    }
}
