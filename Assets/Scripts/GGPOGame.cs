using UnityEngine;
using GGPOSharp;
using System.Runtime.InteropServices;

public class GGPOGame : MonoBehaviour, IGameInterface
{
    public BallController ball;
    public Rigidbody rb;

    public void AdvanceFrame(GGPOPlayerHandle[] players, byte[][] inputs)
    {
        PlayerInput input = ByteArrayToInput(inputs[0]);
        ball.ApplyInput(input);
    }

    public void SaveGameState(out object state, out int checksum)
    {
        GameState s = new GameState()
        {
            ballY = ball.transform.position.y,
            ballVelocityY = rb.velocity.y,
            isGrounded = ball.isGrounded
        };
        state = s;
        checksum = s.GetHashCode();
    }

    public void LoadGameState(object state)
    {
        GameState s = (GameState)state;
        ball.transform.position = new Vector3(0, s.ballY, 0);
        rb.velocity = new Vector3(0, s.ballVelocityY, 0);
        ball.isGrounded = s.isGrounded;
    }

    public int? OnEvent(GGPOEvent e)
    {
        Debug.Log($"GGPO Event: {e.code}");
        return null;
    }

    private PlayerInput ByteArrayToInput(byte[] data)
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        PlayerInput input = (PlayerInput)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(PlayerInput));
        handle.Free();
        return input;
    }
}
