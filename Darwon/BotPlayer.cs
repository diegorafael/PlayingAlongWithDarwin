using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace Darwon
{
    internal class BotPlayer
    {
        int attempts = 1;
        private int BestScore = -1;
        private readonly Chromosome _dna;

        public BotPlayer(Chromosome dna)
        {
            _dna = dna;
        }

        public async Task Play(string gameUrl)
        {
            try
            {
                var edgeOptions = new EdgeOptions();

                //edgeOptions.AddArgument("--headless");

                using var webDriver = new EdgeDriver(edgeOptions);
                webDriver.Manage().Window.Maximize();
                webDriver.Url = gameUrl;
                //Console.WriteLine($"Bot '{name}' started a game");

                for (int i = 0; i < attempts; i++)
                {
                    webDriver.Navigate().Refresh();

                    var gameboard = webDriver.FindElement(By.Id("game-board"));
                    var dino = webDriver.FindElement(By.Id("dino"));

                    while (webDriver.FindElements(By.ClassName("game-over")).Count() == 0)
                    {
                        var obstacle = webDriver.FindElements(By.ClassName("obstacle")).FirstOrDefault();

                        if (obstacle is not null)
                        {
                            var distance = obstacle.Location.X - dino.Location.X;
                            var height = obstacle.Location.Y - dino.Location.Y;

                            var actions = _dna.ReactToEnemy(distance, height, webDriver);

                            actions.Perform();
                        }
                    }

                    //Console.WriteLine($"{name} lost the game");
                    var score = Convert.ToInt32(webDriver
                        .FindElement(By.Id("score-text"))
                        .Text
                        .Replace("DISTANCE: ", ""));

                    BestScore = score > BestScore ? score : BestScore;
                    //Console.WriteLine($"Bot '{name}' got {score} on the attempt number {attempt}.");
                }
                webDriver.Close();
                _dna.Fitness = BestScore;
                //Console.WriteLine($"Player DNA : {_dna}");
            }
            catch(Exception e)
            {
            }
        }
    }
}
