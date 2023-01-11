using Fusion;
using System;

namespace Assets.Model
{
    public struct Race : INetworkStruct
    {
        public NetworkString<_4> Id { get; set; }
        public NetworkString<_16> Name { get; set; }
        public int Tokens { get; set; }
        public bool InDecline { get; set; }
    }

    // todo race factory?
}
