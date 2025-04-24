using System.Collections.Generic;

namespace Network
{
    // Frame bazlı input buffer
    public class InputBuffer
    {
        // frameNumber -> (playerId -> inputBits)
        Dictionary<int, Dictionary<int, ulong>> buffer = new Dictionary<int, Dictionary<int, ulong>>();

        // Yeni input ekle
        public void Add(int frameNumber, int playerId, ulong input)
        {
            if (!buffer.TryGetValue(frameNumber, out var dict))
            {
                dict = new Dictionary<int, ulong>();
                buffer.Add(frameNumber, dict);
            }
            dict[playerId] = input;
        }

        // Belirli frame için tüm inputları çek
        public Dictionary<int, ulong> GetInputs(int frameNumber)
        {
            if (buffer.TryGetValue(frameNumber, out var dict))
                return new Dictionary<int, ulong>(dict);
            return new Dictionary<int, ulong>();
        }

        // Gereksiz eski veriyi temizle
        public void PurgeFramesBelow(int minFrame)
        {
            var keys = new List<int>(buffer.Keys);
            foreach (var k in keys)
                if (k < minFrame) buffer.Remove(k);
        }
    }
}