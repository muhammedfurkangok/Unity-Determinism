using System.Collections.Generic;
using InputSystem;
using UnityEngine;

public static class InputRecorder
{
    // Kayıt modunda her frame'de hangi inputların basıldığını saklamak için liste.
    public static List<InputAction> RecordedInputs = new List<InputAction>();

    // Her frame'de gerçek inputları alıp enum kombinasyonuyla kaydeder.
    public static void RecordCurrentInput()
    {
        InputAction actions = InputAction.None;

        if (Input.GetKey(KeyCode.D))
        {
            actions |= InputAction.Right;
        }
        if (Input.GetKey(KeyCode.A))
        {
            actions |= InputAction.Left;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            actions |= InputAction.Jump;
        }

        RecordedInputs.Add(actions);
    }
}