using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Manager))]
public class PostProcessingManager : MonoBehaviour
{
    [SerializeField] private float focusCAS = -40, cASChangeSpeed = 10f;
    private float normalCAS, desirableCAS;

    [SerializeField] private Volume volume;
    private ColorAdjustments cA;
    private Manager mngr;

    private void Awake()
    {
        mngr = GetComponent<Manager>();

        volume.profile.TryGet(out cA);

        normalCAS = cA.saturation.value;

        desirableCAS = normalCAS;
    }

    public void UpdateFunction()
    {
        if (cA.saturation.value != desirableCAS)
        {
            cA.saturation.value = Mathf.MoveTowards(cA.saturation.value, desirableCAS,
                cASChangeSpeed * mngr.GetUnscaledDeltaTime());
        }
    }

    public void SetColorAdjustmentsSaturation(bool isFocusing)
    {
        desirableCAS = isFocusing ? focusCAS : normalCAS;
    }
}
