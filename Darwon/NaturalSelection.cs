namespace Darwon
{
    internal class NaturalSelection
    {
        public IList<IChromosome> Population { get; private set; }
        private Random random;
        
        public int Generation { get; private set; }
        
        public float MutationRate { get; }
        private readonly float targetFitness;
        private readonly int populationSize;
        private readonly int dnaSize;
        private readonly int reproductionPercent;

        public NaturalSelection(Configuration configuration)
            : this(configuration, default)
        {

        }

        public NaturalSelection(Configuration configuration, params float[] exemplar)
        {
            Generation = 1;
            MutationRate = configuration.MutationRate;
            Population = new List<IChromosome>();
            targetFitness = configuration.TargetFitnessValue;
            populationSize = configuration.PopulationSize;
            reproductionPercent = configuration.ReproductionPercent;
            dnaSize = configuration.DnaSize;
            random = new Random();

            for (int i = 0; i < populationSize; i++)
                Population.Add(new Chromosome(random, configuration.DnaSize));

            if (exemplar != null)
                Population[0] = new Chromosome(random, exemplar);
        }

        public bool Achieved
        {
            get => targetFitness <= Population.Max(individual => individual.Score);
        }

        public void Evolve()
        {
            if (Achieved)
                throw new InvalidOperationException("Already found the solution!");

            // Ranking
            var intermediatePopulation = GetIntermediatePopulation(Convert.ToInt32(populationSize * (reproductionPercent / 100f)));

            Population.Clear();

            while (Population.Count < populationSize)
            {
                var (parent1, parent2) = SelectParents(intermediatePopulation);

                Population.Add(parent1.Crossover(parent2, MutationRate));
                Population.Add(parent2.Crossover(parent1, MutationRate));
            }

            Generation++;
        }

        private IEnumerable<IChromosome> GetIntermediatePopulation(int numberOfSubjects)
        {
            IList<IChromosome> intermediatePopulation = new List<IChromosome>();

            var averageScore = Population.Average(s => s.Score);

            foreach (var subject in Population)
                subject.Fitness = (float)(subject.Score / averageScore);

            var indexes = new List<int>();
            for (int i = 0; i < Population.Count(); i++)
                indexes.AddRange(Enumerable.Repeat(i, Convert.ToInt32(Population.ElementAt(i).Fitness)));

            for (int i = 0; i < numberOfSubjects; i++)
            {
                var selectedIndex = random.Next(indexes.Count);
                intermediatePopulation.Add(Population.ElementAt(indexes.ElementAt(selectedIndex)));
            }

            return intermediatePopulation;
        }

        private (IChromosome, IChromosome) SelectParents(IEnumerable<IChromosome> population)
        {
            IList<IChromosome> candidates = new List<IChromosome>(population)
                .Where(s => s.Fitness > 0)
                .ToList();

            if (candidates.Count() < 1)
                return (new Chromosome(random, dnaSize), new Chromosome(random, dnaSize));

            if (candidates.Count() < 2)
                return (candidates.First(), new Chromosome(random, dnaSize));

            if (candidates.Count() % 2 != 0)
                candidates.Add(new Chromosome(random, dnaSize) { Fitness = 1 });
            
            var indexes = new List<int>();
            for (int i = 0; i < candidates.Count(); i++)
                indexes.AddRange(Enumerable.Repeat(i, Convert.ToInt32(candidates.ElementAt(i).Fitness)));

            var selectedIndex = random.Next(indexes.Count);
            var selectedElement = candidates.ElementAt(indexes.ElementAt(selectedIndex));
            indexes = indexes.Where(i => i != indexes.ElementAt(selectedIndex)).ToList();
            var selectedPair = random.Next(indexes.Count);
            
            return (selectedElement, candidates.ElementAt(indexes.ElementAt(selectedPair)));
        }

    }

    class Configuration
    {
        public int PopulationSize { get; set; }
        public int ReproductionPercent { get; set; }
        public int DnaSize { get; set; }
        public float MutationRate { get; set; }
        public float TargetFitnessValue { get; set; }
    }
}
