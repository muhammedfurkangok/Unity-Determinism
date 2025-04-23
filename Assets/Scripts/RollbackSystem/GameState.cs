using SoftFloat;

namespace PhysicSystem
{
    // Oyun durumunu saklamak için kullanılacak veri yapısı
    public struct GameState
    {
        public int Frame;
        public BallData BallData;

        // Deep copy oluşturmak için constructor
        public GameState(int frame, BallData ballData)
        {
            Frame = frame;
            BallData = ballData.Clone();
        }
    }

    // Oyuncu girdilerini saklamak için kullanılacak veri yapısı

}