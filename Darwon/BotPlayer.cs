using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Interactions;
using System.Drawing;
using System.Web;

namespace Darwon
{
    public interface IPlayer
    {
        int BestScore { get; }
        Task Play(string gameUrl, IChromosome chromosome);
    }

    internal class BotPlayer : IPlayer
    {
        int attempts = 1;
        public int BestScore { get; private set; } = -1;
        private TimeSpan clockLimit = TimeSpan.FromMilliseconds(50);
        public BotPlayer()
        {
        }

        public async Task Play(string gameUrl, IChromosome chromosome)
        {
            try
            {
                var edgeOptions = new EdgeOptions();
                
                edgeOptions.AddArgument("--headless");

                using var webDriver = new EdgeDriver(edgeOptions);
                webDriver.Manage().Window.Size = new Size(1200, 600);
                webDriver.Url = gameUrl;
                //Console.WriteLine($"Bot '{name}' started a game");

                var javascript1 = $"var o = document.createElement('div'); o.innerHTML = 'Subject DNA: {HttpUtility.JavaScriptStringEncode(chromosome.ToString(), false)}'; document.body.append(o);";
                var javascript2 = $"var o = document.createElement('div'); o.classList.add('debug'); document.body.append(o);";

                DateTime lastExecution = DateTime.Now;

                for (int i = 0; i < attempts; i++)
                {
                    var reactions = 0;
                    webDriver.Navigate().Refresh();
                    webDriver.ExecuteScript(javascript1);
                    webDriver.ExecuteScript(javascript2);

                    var gameboard = webDriver.FindElement(By.Id("game-board"));
                    var dino = webDriver.FindElement(By.Id("dino"));

                    while (webDriver.FindElements(By.ClassName("game-over")).Count() == 0)
                    {
                        if (DateTime.Now.TimeOfDay - lastExecution.TimeOfDay > clockLimit)
                        {
                            var obstacle = webDriver
                            .FindElements(By.ClassName("obstacle"))
                            .Where(obstacle => obstacle.Location.X >= dino.Location.X + dino.Size.Width)
                            .FirstOrDefault();

                            if (obstacle is not null)
                            {
                                var distanceX = obstacle.Location.X - dino.Location.X;
                                var distanceY = obstacle.Location.Y - dino.Location.Y;
                                var obstacleHeight = obstacle.Size.Height;
                                var obstacleWidth = obstacle.Size.Width;
                                var dinoHeight = dino.Size.Height;

                                var dnaOutput = chromosome.Evaluate(distanceX, distanceY, obstacleHeight, obstacleWidth, dinoHeight);

                                var javascript3 = $"var dbg = document.getElementsByClassName('debug'); dbg[0].innerHTML = 'DistanceX: {distanceX} | DistanceY: {distanceY} | Output: {dnaOutput[0]}, {dnaOutput[1]}'";

                                webDriver.ExecuteScript(javascript3);
                                var action = DecodeReactions(dnaOutput, webDriver);

                                action.Perform();

                                reactions++;
                                lastExecution = DateTime.Now;
                            }
                        }
                    }

                    var screenshot = webDriver.GetScreenshot();
                    //Console.WriteLine($"{name} lost the game");
                    var score = Convert.ToInt32(webDriver
                        .FindElement(By.Id("score-text"))
                        .Text
                        .Replace("DISTANCE: ", ""));

                    screenshot.SaveAsFile($"./screenshot-botplayer-{chromosome.GetHashCode()}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}-score-{score}.jpg");

                    if(score > BestScore)
                        BestScore = score;
                }
                webDriver.Close();

                chromosome.Score = BestScore;
            }
            catch (Exception e)
            {
            }
        }

        private Actions DecodeReactions(bool[] reactions, IWebDriver webDriver)
        {
            const int DISTANCE_X_INDEX = 0;
            const int DISTANCE_Y_INDEX = 1;
            const int OBSTACLE_HEIGHT_INDEX = 2;
            const int OBSTACLE_WIDTH_INDEX = 3;
            const int DINO_HEIGHT_INDEX = 4;

            var actions = new Actions(webDriver);

            for (int i = 0; i < reactions.Length; i++)
                switch (i)
                {
                    case DISTANCE_X_INDEX:
                        if (reactions[i])
                            actions.SendKeys(Keys.ArrowUp);
                        break;
                    case DISTANCE_Y_INDEX:
                        if (reactions[i])
                            actions.KeyDown(Keys.ArrowDown);
                        break;
                    case OBSTACLE_HEIGHT_INDEX:
                        if (reactions[i])
                            actions.SendKeys(Keys.ArrowUp);
                        break;
                    case OBSTACLE_WIDTH_INDEX:
                        if (reactions[i])
                            actions.KeyUp(Keys.ArrowDown);
                        break;
                    case DINO_HEIGHT_INDEX:
                        if (reactions[i])
                            actions.KeyUp(Keys.ArrowDown);
                        break;
                }

            return actions;
        }
    }
}
