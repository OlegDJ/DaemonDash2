using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Manager))]
public class UIManager : MonoBehaviour
{
    [SerializeField] private float alphaChangeSpeed = 0.05f;
    [SerializeField] private float brightAlpha = 80f, paleAlpha = 40f;
    private float desirableAlpha;

    private Color curCrosshairColor;

    [SerializeField] private Slider healthBar, energyBar, dashCooldownBar;
    [SerializeField] private Image focusCrosshair;
    [SerializeField] private TextMeshProUGUI focusInfoText;
    private Manager mngr;

    [SerializeField] private TextMeshProUGUI fpsText;
    private int fps, fpsAccumulator;
    private float fpsNextPeriod = 0;
    const float fpsMeasurePeriod = 0.5f;

    [SerializeField] private TextMeshProUGUI curQualityText;

    private void Awake()
    {
        mngr = GetComponent<Manager>();

        curCrosshairColor = focusCrosshair.color;

        TurnCrosshair(false);

        fpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
    }

    public void UpdateFunction()
    {
        if (curCrosshairColor.a != desirableAlpha)
        {
            curCrosshairColor.a = Mathf.MoveTowards(curCrosshairColor.a, desirableAlpha,
                alphaChangeSpeed * mngr.GetUnscaledDeltaTime());
            focusCrosshair.color = curCrosshairColor;
            focusInfoText.color = curCrosshairColor;
        }

        fpsAccumulator++;
        if (Time.realtimeSinceStartup > fpsNextPeriod)
        {
            fps = (int)(fpsAccumulator / fpsMeasurePeriod);
            fpsAccumulator = 0;
            fpsNextPeriod += fpsMeasurePeriod;
            fpsText.SetText($"{fps} FPS");
        }
    }

    public void SetHealthBarMaxValue(float _value)
    {
        healthBar.maxValue = _value;
        healthBar.value = _value;
    }

    public void SetEnergyBarMaxValue(float _value)
    {
        energyBar.maxValue = _value;
        energyBar.value = _value;
    }

    public void SetDashCooldownBarMaxValue(float _value)
    {
        dashCooldownBar.maxValue = _value;
        dashCooldownBar.value = 0f;
    }

    public void SetHealthBarValue(float _value) { healthBar.value = _value; }

    public void SetEnergyBarValue(float _value) { energyBar.value = _value; }

    public void SetDashCooldownBarValue(float _value) { dashCooldownBar.value = _value; }

    public void TurnCrosshair(bool _on) { desirableAlpha = _on ? paleAlpha : 0f; }

    public void ChangeCrosshairAlpha(bool _bright) { desirableAlpha = _bright ? brightAlpha : paleAlpha; }

    public void SetFocusInfoText(string _text) { focusInfoText.text = _text; }

    public void SetCurQualityText(string _text) { curQualityText.text = _text; }
}
