using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Physic;
using TMPro;

public class RollbackTester : MonoBehaviour
{
    [SerializeField] private BallVisualSoftFloat ball;
    [SerializeField] private Button rollbackButton;
    [SerializeField] private Slider rollbackFramesSlider;
    [SerializeField] private TextMeshProUGUI rollbackFramesText;
    
    private int framesToRollback = 60;
    
    void Start()
    {
        if (rollbackButton != null)
        {
            rollbackButton.onClick.AddListener(TriggerRollback);
        }
        
        if (rollbackFramesSlider != null)
        {
            rollbackFramesSlider.onValueChanged.AddListener(SetRollbackFrames);
            rollbackFramesSlider.value = framesToRollback;
            UpdateFramesText();
        }
    }
    
    public void TriggerRollback()
    {
        if (ball != null)
        {
            ball.TestRollback(framesToRollback);
        }
    }
    
    private void SetRollbackFrames(float value)
    {
        framesToRollback = Mathf.RoundToInt(value);
        UpdateFramesText();
    }
    
    private void UpdateFramesText()
    {
        if (rollbackFramesText != null)
        {
            rollbackFramesText.text = $"Rollback Frames: {framesToRollback}";
        }
    }
}