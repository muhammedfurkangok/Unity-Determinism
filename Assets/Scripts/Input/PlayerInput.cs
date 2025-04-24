using SoftFloat;

public struct PlayerInput
{
    public int Frame;
    public sfloat HorizontalInput;
    public bool JumpInput;

    public PlayerInput(int frame, sfloat horizontalInput, bool jumpInput)
    {
        Frame = frame;
        HorizontalInput = horizontalInput;
        JumpInput = jumpInput;
    }
}