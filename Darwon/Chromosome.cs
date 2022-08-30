using System.Text;

namespace Darwon
{
    public interface IChromosome
    {
        int Score { get; set; }
        float Fitness { get; set; }
        IEnumerable<float> Genes { get; }
        bool[] Evaluate(params float[] inputs);
        IChromosome Crossover(IChromosome pair, float mutationRate);
    }
    public class Chromosome : IChromosome
    {
        public IEnumerable<float> Genes { get; }
        private int genesCount;

        private readonly Random _random;

        public int Score { get; set; }
        public float Fitness { get; set; }

        public Chromosome(Random random, params float[] genes)
        {
            _random = random;
            Genes = genes;

            genesCount = genes.Length;
        }
        public Chromosome(Random random, int numberOfGenes)
            : this(random, GenerateGenes(random, numberOfGenes).ToArray())
        {
        }
        private static float RandomGene(Random random)
            => (float)(random.NextDouble() * 1000f * (random.NextDouble() < 0.5 ? -1 : 1));

        public bool[] Evaluate(params float[] inputs)
        {
            if (inputs.Length != genesCount)
                throw new InvalidOperationException("The number of inputs must match the number of genes");

            var results = new bool[inputs.Length];

            for (int i = 0; i < results.Length; i++)
                results[i] = inputs[i] < Genes.ElementAt(i);

            return results;
        }

        public IChromosome Crossover(IChromosome other, float mutationRate)
        {
            var cutPoint = _random.Next(genesCount);
            var newElementDna= new List<float>();

            for (int i = 0; i < genesCount; i++)
                if (i <= cutPoint)
                    newElementDna.Add(Genes.ElementAt(i));
                else
                    newElementDna.Add(other.Genes.ElementAt(i));

            MutateDna(newElementDna, mutationRate);

            return new Chromosome(_random, newElementDna.ToArray());
        }

        private void MutateDna(IList<float> newElementDna, float mutationRate)
        {
            var willMutate = _random.NextDouble() < mutationRate;

            if (willMutate)
            {
                var mutatedGeneIndex = _random.Next(newElementDna.Count());
                newElementDna[mutatedGeneIndex] += _random.Next(-50,50);
            }
        }

        private static IEnumerable<float> GenerateGenes(Random random, int numberOfGenes)
        {
            var genes = new List<float>();
            for (int i = 0; i < numberOfGenes; i++)
                genes.Add(RandomGene(random));

            return genes;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"|--------Score: {Score, 4}--------|");
            for (int i = 0; i < genesCount; i++)
                sb.AppendLine($"| Gene {i + 1, 2} | {Genes.ElementAt(i)} |");
            sb.AppendLine($"|---------------------------|");

            return sb.ToString();
        }
    }
}
