using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GGPOGame ggpoGame;

    void Start()
    {
        // GGPO başlatılır (UnityGGPO'dan alınmış config ile)
        GGPOWrapper.StartSession(ggpoGame, 2);
    }

    void Update()
    {
        // Local input alınır
        PlayerInput input = new PlayerInput()
        {
            jumpPressed = Input.GetKeyDown(KeyCode.Space)
        };

        // GGPO’ya input verilir
        GGPOWrapper.SubmitLocalInput(input);

        // GGPO frame ilerletilir
        GGPOWrapper.DoFrame();
    }
}
