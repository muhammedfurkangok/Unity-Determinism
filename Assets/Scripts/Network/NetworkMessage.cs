using System;

namespace Network
{
    // Tekrar oynatmak üzere gönderilen veri: frame ve 64-bit input
    [Serializable]
    public struct NetworkMessage
    {
        public int frameNumber;
        public ulong inputBits;
        public int fromPlayerId;
    }
}