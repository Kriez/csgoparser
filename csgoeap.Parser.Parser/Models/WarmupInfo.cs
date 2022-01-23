namespace csgoeap.Parser.Parser.Models
{
    public class WarmupInfo
    {
        public WarmupInfo()
        {
            StartedWithDefuseKit = false;

        }

        public long SteamId { get; set; }
        public bool StartedWithDefuseKit { get; set; }
    }
}
