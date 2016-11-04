namespace DigitRecognition
{
    public class Observation
    {
        public Observation(string label, int[] pixels)
        {
            this.Label = label;
            this.Pixels = pixels;
        }

        public int[] Pixels { get; private set; }

        public string Label { get; private set; }
    }
}
