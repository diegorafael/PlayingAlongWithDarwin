using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Interactions;
using System.Text;

namespace Darwon
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var configuration = new Configuration
            {
                MutationRate = 0.15f,
                PopulationSize = 10,
                TargetFitnessValue = 1600,
                ReproductionPercent = 60,
                DnaSize = 5
            };

            var naturalSelection = new NaturalSelection(configuration);
            //var naturalSelection = new NaturalSelection(configuration, 308.7113f, -620.9605f);
            List<Task> players = new List<Task>();
            
            if(File.Exists("./results.txt"))
                File.Delete("./results.txt");
            
            StringBuilder stringBuilder = new StringBuilder();

            do
            {
                stringBuilder.Clear();

                stringBuilder.AppendLine($"-     Generation {naturalSelection.Generation}    -\n\nSubjects:");
                foreach (var subject in naturalSelection.Population)
                    stringBuilder.AppendLine($"- {subject}");

                Console.WriteLine(stringBuilder.ToString());

                foreach (var subject in naturalSelection.Population)
                    players.Add(Task.Run(() => new BotPlayer().Play("http://localhost:5500", subject)));

                Task.WhenAll(players).GetAwaiter().GetResult();
                stringBuilder.AppendLine($"\n===============================================================================================\n\n\n");
                stringBuilder.AppendLine($"Avg Fit: {naturalSelection.Population.Average(p => p.Score)}\n  Best players:");
                var bests = naturalSelection.Bests;
                foreach (var player in bests)
                    stringBuilder.AppendLine($"    {player}");
                stringBuilder.AppendLine($"\n===============================================================================================");

                var finalText = stringBuilder.ToString();
                Console.WriteLine(finalText);
                
                File.AppendAllText("./results.txt", finalText);
                
                if(!naturalSelection.Achieved)
                    naturalSelection.Evolve();

                players.Clear();
            } while (!naturalSelection.Achieved);

            Console.WriteLine($"The generation {naturalSelection.Generation} got the expected results.");

            Console.ReadLine();
        }
    }
}