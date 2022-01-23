namespace csgoeap.Parser.Parser.Models
{
    public class ParsedFlashes
    {
        public int Flashed { get; set; }
        public float? FlashedDuration { get; set; }
        public int SelfFlashed { get; set; }
        public int EnemiesFlashed { get; set; }
        public int FriendlyFlashed { get; set; }
        public void Combine(ParsedFlashes parsedFlashes)
        {
            Flashed += Flashed;
            FlashedDuration += FlashedDuration;
            SelfFlashed += SelfFlashed;
            EnemiesFlashed += EnemiesFlashed;
            FriendlyFlashed += FriendlyFlashed;
        }
    }
}
