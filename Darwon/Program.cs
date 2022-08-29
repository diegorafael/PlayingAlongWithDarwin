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
                PopulationSize = 5,
                TargetFitnessValue = 1000
            };

            //var naturalSelection = new NaturalSelection(configuration);
            var naturalSelection = new NaturalSelection(configuration, new Chromosome(new Gene(809.61615f), new Gene(75.7921f)));
            List<Task> players = new List<Task>();

            do
            {
                foreach (var individual in naturalSelection.Population)
                    players.Add(Task.Run(() => new BotPlayer(individual).Play("http://localhost:5500")));

                Task.WhenAll(players).GetAwaiter().GetResult();
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"\n===============================================================================================\n\n\n");
                stringBuilder.AppendLine($"Generation {naturalSelection.Generation}\n\n  Avg Fit: {naturalSelection.Population.Average(p => p.Fitness)}\n  Best players:");
                var bests = naturalSelection.Bests;
                foreach (var player in bests)
                    stringBuilder.AppendLine($"    {player}");
                stringBuilder.AppendLine($"\n\n  Last best player ever: {naturalSelection.BestEver}\n\n\n");
                stringBuilder.AppendLine($"\n===============================================================================================");

                var finalText = stringBuilder.ToString();
                Console.WriteLine(finalText);
                
                File.WriteAllText("./results.txt", $"Best result: {finalText}");
                
                if(!naturalSelection.Achieved)
                    naturalSelection.Evolve();

                players.Clear();
            } while (!naturalSelection.Achieved);

            Console.WriteLine($"The generation {naturalSelection.Generation} got the expected results.");

            Console.ReadLine();
        }

        static void PlayGame()
        {
            var webDriver = new EdgeDriver();
            webDriver.Url = "http://localhost:5500";

            while (true)
            {
                var gameboard = webDriver.FindElement(By.Id("game-board"));

                while (webDriver.FindElements(By.ClassName("game-over")).Count() == 0)
                {
                    Console.WriteLine("Jump");

                    new Actions(webDriver)
                    .MoveToElement(gameboard)
                    .SendKeys(Keys.ArrowUp)
                    .Perform();
                    Thread.Sleep(800);
                }

                Console.WriteLine("Game Over! Restarting in 4 seconds...");
                Thread.Sleep(4000);
                webDriver.Navigate().Refresh();
                Thread.Sleep(500);
            }

        }
    }
}