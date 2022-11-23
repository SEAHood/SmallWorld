using Fusion;

namespace Assets.Model
{
    public struct Power : INetworkStruct
    {
        public NetworkString<_16> Name { get; set; }
        public int Tokens { get; set; }
    }
}
