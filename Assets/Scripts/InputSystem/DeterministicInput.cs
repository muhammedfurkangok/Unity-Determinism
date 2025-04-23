using System.Collections.Generic;
using InputSystem;

public static class DeterministicInput
{
    // Kayıtlı input dizisini saklıyoruz.
    private static List<InputAction> inputSequence = new List<InputAction>();

    private static int currentFrame = 0;

    // Kayıtlı input dizisini ayarlamak için.
    public static void SetInputSequence(List<InputAction> sequence)
    {
        inputSequence = sequence;
        currentFrame = 0;
    }

    // Mevcut frame için input bilgisini döndürür.
    public static InputAction GetInputForCurrentFrame()
    {
        // Eğer input dizisi boşsa, default değer olarak None döndürüyoruz.
        if (inputSequence == null || inputSequence.Count == 0)
        {
            return InputAction.None;
        }

        // Eğer currentFrame indexi diziyi aşıyorsa, son elemanı döndürürüz.
        if (currentFrame >= inputSequence.Count)
        {
            return inputSequence[inputSequence.Count - 1];
        }

        return inputSequence[currentFrame];
    }

    // Her frame sonunda frame sayacını artırır.
    public static void AdvanceFrame()
    {
        currentFrame++;
    }

    // İsteğe bağlı olarak frame sayacını sıfırlar.
    public static void Reset()
    {
        currentFrame = 0;
    }
}