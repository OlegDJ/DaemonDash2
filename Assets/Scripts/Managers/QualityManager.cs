using UnityEngine;

public class QualityManager : MonoBehaviour
{
    private UIManager ui;

    private void Awake()
    {
        ui = GetComponent<UIManager>();
    }

    public void SetQuality(Quality _quality)
    {
        switch (_quality)
        {
            case Quality.Low:
                QualitySettings.SetQualityLevel(0, true);
                break;
            case Quality.Medium:
                QualitySettings.SetQualityLevel(1, true);
                break;
            case Quality.High:
                QualitySettings.SetQualityLevel(2, true);
                break;
            case Quality.Ultra:
                QualitySettings.SetQualityLevel(3, true);
                break;
        }
        ui.SetCurQualityText(QualitySettings.names[QualitySettings.GetQualityLevel()]);
    }
}

public enum Quality
{
    Low,
    Medium,
    High,
    Ultra
}