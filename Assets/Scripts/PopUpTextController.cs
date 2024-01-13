using UnityEngine;
using TMPro;

public class PopUpTextController : MonoBehaviour
{
    [SerializeField] private float durationTime = 0.5f, fadeSpeed = 5f, moveSpeed = 1f;

    private Color textFadeColor;
    private TextMeshPro textMeshPro;
    private Transform mainCameraTransform;

    public void SetUp(string text)
    {
        textMeshPro = GetComponent<TextMeshPro>();
        mainCameraTransform = Camera.main.transform;
        textMeshPro.SetText(text);
        textFadeColor = textMeshPro.color;
    }

    private void LateUpdate()
    {
        transform.LookAt(2 * transform.position - mainCameraTransform.position);
        transform.position += new Vector3(0f, moveSpeed * Time.deltaTime, 0f);

        durationTime -= Time.deltaTime;
        if (durationTime <= 0)
        {
            textFadeColor.a -= fadeSpeed * Time.deltaTime;
            textMeshPro.color = textFadeColor;
            if (textFadeColor.a <= 0f) Destroy(gameObject);
        }
    }
}
