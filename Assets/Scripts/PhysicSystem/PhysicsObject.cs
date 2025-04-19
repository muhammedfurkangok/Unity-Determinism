using SoftFloat;

public class PhysicsObject
{
    public FpVector2 Position;
    public  Velocity;
    public bool IsStatic;

    public PhysicsObject(FpVector2 position)
    {
        Position = position;
        Velocity = FpVector2.zero;
        IsStatic = false;
    }

    public void Step(Fp deltaTime)
    {
        if (IsStatic) return;
        Position += Velocity * deltaTime;
    }
}