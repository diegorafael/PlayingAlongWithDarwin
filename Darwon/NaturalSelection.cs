using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Darwon
{
    internal class NaturalSelection
    {

        private Random random;
        public List<Chromosome> Population { get; private set; }
        public int Generation { get; private set; }
        public float MutationRate { get; set; }
        private float targetFitness;
        private int populationSize;
        public IEnumerable<Chromosome> Bests
        {
            get => Population
                .Where(c => BestEver == null ? true : c.Fitness >= BestEver.Fitness * 0.8)
                .OrderByDescending(individual => individual.Fitness)
                .Take(populationSize / 2);
        }
        public Chromosome? BestEver { get; private set; }
        public NaturalSelection(Configuration configuration)
            : this(configuration, default)
        {

        }

        public NaturalSelection(Configuration configuration, Chromosome? exemplar)
        {
            Generation = 1;
            MutationRate = configuration.MutationRate;
            Population = new List<Chromosome>();
            targetFitness = configuration.TargetFitnessValue;
            populationSize = configuration.PopulationSize;
            random = new Random();


            for (int i = 0; i < populationSize; i++)
            {
                var individual = Chromosome.Spawn(random);
                if (exemplar != null)
                    individual = individual.Crossover(random, exemplar, MutationRate);

                Population.Add(individual);
            }
        }

        public bool Achieved
        {
            get => targetFitness <= Population.Max(individual => individual.Fitness);
        }

        public void Evolve()
        {
            if (Achieved)
                throw new InvalidOperationException("Already found the solution!");

            var bests = Bests.ToList();

            if (BestEver == null || bests.Count > 0 && bests.ElementAt(0).Fitness > BestEver.Fitness)
                BestEver = bests.ElementAt(0);

            Population.Clear();
            while (Population.Count < populationSize)
            {
                var (parent1, parent2) = SelectParents(bests);
                Population.Add(parent1.Crossover(random, parent2, MutationRate));
            }

            Generation++;
        }

        private (Chromosome, Chromosome) SelectParents(IEnumerable<Chromosome> bests)
        {
            if(bests.Count() == 0)
                return (BestEver!, Chromosome.Spawn(random));

            if (bests.Count() == 1)
                return (BestEver!, random.NextDouble() < 0.5 ? bests.ElementAt(0)! : Chromosome.Spawn(random));

            var indexes = new List<int>();
            for (int i = 0; i < bests.Count(); i++)
                indexes.AddRange(Enumerable.Repeat(i, Convert.ToInt32(bests.ElementAt(i).Fitness)));

            var selectedIndex = random.Next(indexes.Count);
            var selectedElement = bests.ElementAt(indexes.ElementAt(selectedIndex));
            indexes = indexes.Where(i => i != indexes.ElementAt(selectedIndex)).ToList();
            var selectedPair = random.Next(indexes.Count);
            
            return (selectedElement, bests.ElementAt(indexes.ElementAt(selectedPair)));
        }

    }

    class Configuration
    {
        public int PopulationSize { get; set; }
        public float MutationRate { get; set; }
        public float TargetFitnessValue { get; set; }
    }


    public class Chromosome
    {
        public Gene Distance { get; private set; }
        public Gene Height { get; private set; }
        bool isDown = false;

        public float Fitness { get; set; }

        private Chromosome()
        {
        }

        public Chromosome(Gene distance, Gene height)
        {
            Distance = distance;
            Height = height;
        }

        public Actions ReactToEnemy(float distance, float height, IWebDriver webDriver)
        {
            var actions = new Actions(webDriver);
            var actionRange = 1000 - Distance.Tolerance;
            var behavior = 100 - Height.Tolerance;

            if (distance <= actionRange)
            {
                if (height <= behavior)
                {
                    actions.SendKeys(Keys.ArrowUp);
                    actions.Pause(TimeSpan.FromMilliseconds(500));
                }
                else
                {
                    if (!isDown)
                    {
                        actions.KeyDown(Keys.ArrowDown);
                        isDown = true;
                    }
                }
            }
            else
            {
                if (isDown)
                {
                    actions.KeyUp(Keys.ArrowDown);
                    isDown = false;
                }
            }

            return actions;
        }

        public Chromosome Crossover(Random random, Chromosome other, float mutationRate)
        {
            var lucky1 = random.NextDouble();
            var lucky2 = random.NextDouble();

            var distanceTolerance = (lucky1 < 0.5 ? Distance.Tolerance : other.Distance.Tolerance) + (MutationFactor(random, mutationRate) * 500);
            var heightTolerance = (lucky2 < 0.5 ? Height.Tolerance : other.Height.Tolerance) + (MutationFactor(random, mutationRate) * 100);

            var child = new Chromosome(
                new Gene(distanceTolerance),
                new Gene(heightTolerance));

            Console.WriteLine($"New individual: {child}");

            return child;
        }

        public float MutationFactor(Random random, float mutationRate)
        {
            var rate = (float)random.NextDouble();

            if (rate <= mutationRate)
                return rate;
            else
                return 0f;
        }

        public static Chromosome Spawn(Random random)
            => new Chromosome()
            {
                Distance = new Gene((float)random.NextDouble() * 1000f * (random.NextDouble() > 0.5 ? -1 : 1)),
                Height = new Gene((float)random.NextDouble() * 100f * (random.NextDouble() > 0.5 ? -1 : 1))
            };

        public override string ToString()
            => $"D: {Distance.Tolerance} | H: {Height.Tolerance} | Is down: {isDown} | Fitness: {Fitness}";
    }

    public struct Gene
    {
        public float Tolerance { get; private set; }

        public Gene(float tolerance)
        {
            Tolerance = tolerance;
        }
    }
}
