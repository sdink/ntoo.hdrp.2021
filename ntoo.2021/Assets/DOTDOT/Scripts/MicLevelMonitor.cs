using UnityEngine;
using UnityEngine.UI;

public class MicLevelMonitor : MonoBehaviour
{
  [SerializeField]
  private MicManager micManager;

  [SerializeField]
  [Range(0, 1)]
  [Tooltip("Maximum volume reading this indicator should show")]
  private float maxVolume = 1;

  [Header("UI Components")]
  [SerializeField]
  RectTransform levelIndicator;

  [SerializeField]
  Image levelIndicatorVisual;

  [SerializeField]
  Slider thresholdController;

  private void OnEnable()
  {
    if (maxVolume < micManager.MicThreshold)
    {
      maxVolume = micManager.MicThreshold;
    }

    UpdateThresholdLevel(micManager.MicThreshold);
  }

  public void UpdateThresholdLevel(float level)
  {
    float indicatorPos = level / maxVolume;
    Debug.Log("[Mic Level Monitor] Updating threshold indicator to " + indicatorPos);
    thresholdController.maxValue = maxVolume;
    thresholdController.minValue = 0;
    thresholdController.value = level;
  }

  public void OnThresholdControllerValueChanged(float value)
  {
    micManager.MicThreshold = value;
  }

  public void UpdateInputLevel(float level)
  {
    levelIndicator.anchorMax = new Vector2(1, level / maxVolume);

    if (level > micManager.MicThreshold)
    {
      levelIndicatorVisual.color = Color.red;
    }
    else
    {
      levelIndicatorVisual.color = Color.green;
    }
  }
}
