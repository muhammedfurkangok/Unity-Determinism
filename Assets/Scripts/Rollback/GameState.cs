using SoftFloat;

namespace PhysicSystem
{
    public struct GameState
    {
        public int Frame;
        public BallData BallData;

        public GameState(int frame, BallData ballData)
        {
            Frame = frame;
            BallData = ballData.Clone();
        }
    }


}