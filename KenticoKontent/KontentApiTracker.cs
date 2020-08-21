using Core.KenticoKontent.Services;

namespace KenticoKontent
{
    public class KontentApiTracker : IKontentApiTracker
    {
        public int ApiCalls { get; set; }
    }
}