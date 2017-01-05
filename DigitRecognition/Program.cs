using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitRecognition
{
    class Program
    {
        static void Main(string[] args)
        {
            var distance = new ManhattanDistance();
            var classifier = new BasicClassifier(distance);

            var trainingPath = ".\\Data\\trainingsample.csv";
            var training = DataReader.ReadObservations(trainingPath);
            classifier.Train(training);

            var validationPath = ".\\Data\\validationsample.csv";
            var validation = DataReader.ReadObservations(validationPath);

            var correct = Evaluator.Correct(validation, classifier);
            Console.WriteLine($"Correctly classified: {correct:P2}");
            
            Console.ReadLine();
        }
    }
}
