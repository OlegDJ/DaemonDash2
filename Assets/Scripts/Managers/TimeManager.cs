using UnityEngine;

[RequireComponent(typeof(Manager))]
public class TimeManager : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float slowedTimeScale = 0.2f;
    [SerializeField] private float timeChangeSpeed = 0.1f;
    private float targetTimeScale;

    private Manager mngr;

    private void Awake()
    {
        mngr = GetComponent<Manager>();

        targetTimeScale = 1f;
    }

    public void UpdateFunction()
    {
        if (Time.timeScale != targetTimeScale)
        {
            Time.timeScale = Mathf.Clamp01(
                Mathf.MoveTowards(Time.timeScale, targetTimeScale, timeChangeSpeed * mngr.GetUnscaledDeltaTime()));
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
    }

    public void SlowDownTime() { targetTimeScale = slowedTimeScale; }

    public void SpeedUpTime() { targetTimeScale = 1f; }
}
