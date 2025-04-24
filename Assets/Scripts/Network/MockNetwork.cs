using System;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    // Basit UDP simülasyonu: delay, jitter, packet loss
    public class MockNetwork
    {
        public float meanLatencyMs = 100f;
        public float jitterMs = 30f;
        [Range(0,100)] public float packetLossPercent = 5f;

        // (targetPlayerId, message)
        public event Action<int, NetworkMessage> OnReceive;

        struct InFlight
        {
            public int deliverTo;
            public NetworkMessage msg;
            public float deliverTime; // Time.time + delay
        }

        List<InFlight> inFlight = new List<InFlight>();
        System.Random rng = new System.Random();

        public void Send(int fromId, int toId, int frame, ulong input)
        {
            // Packet loss
            if (rng.NextDouble() < packetLossPercent / 100.0) return;

            // Compute delay
            float baseDelay = meanLatencyMs + ((float)(rng.NextDouble() * 2 - 1) * jitterMs);
            float deliverTime = Time.time * 1000f + Mathf.Max(0, baseDelay);

            var msg = new NetworkMessage
            {
                frameNumber = frame,
                inputBits = input,
                fromPlayerId = fromId
            };

            inFlight.Add(new InFlight
            {
                deliverTo = toId,
                msg = msg,
                deliverTime = deliverTime
            });
        }

        public void Update()
        {
            float now = Time.time * 1000f;
            for (int i = inFlight.Count - 1; i >= 0; i--)
            {
                if (inFlight[i].deliverTime <= now)
                {
                    OnReceive?.Invoke(inFlight[i].deliverTo, inFlight[i].msg);
                    inFlight.RemoveAt(i);
                }
            }
        }
    }
}