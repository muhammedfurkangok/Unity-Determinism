using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    private static Dictionary<int, bool> inputPerFrame = new Dictionary<int, bool>();

    private static bool useMockInput = true; // DEBUG için true

    public static void SetUseMockInput(bool value)
    {
        useMockInput = value;
    }

    public static void RegisterInput(int frame)
    {
        bool jumpInput;

        if (useMockInput)
        {
            // TEST: Frame 10, 20 ve 30'da zıpla
            jumpInput = (frame == 10 || frame == 20 || frame == 30);
        }
        else
        {
            // Gerçek input
            jumpInput = Input.GetKey(KeyCode.Space);
        }

        if (!inputPerFrame.ContainsKey(frame))
        {
            inputPerFrame.Add(frame, jumpInput);
        }
    }

    public static bool GetInput(int frame)
    {
        if (inputPerFrame.TryGetValue(frame, out bool input))
            return input;

        return false; // Default input
    }

    public static void ClearAfterFrame(int frame)
    {
        var keysToRemove = new List<int>();

        foreach (var key in inputPerFrame.Keys)
        {
            if (key > frame)
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
        {
            inputPerFrame.Remove(key);
        }
    }

    public static void ForceSetInput(int frame, bool value)
    {
        inputPerFrame[frame] = value;
    }
}