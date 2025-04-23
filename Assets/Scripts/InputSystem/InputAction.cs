using System;

namespace InputSystem
{
    [Flags]
    public enum InputAction
    {
        None = 0,
        Right = 1,
        Left = 2,
        Jump = 4
    }
}