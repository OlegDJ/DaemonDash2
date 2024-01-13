using UnityEngine;

public class RagdollController : MonoBehaviour
{
    private bool isMoveStarted;

    [SerializeField] private Vector3 moveOffset = new(0f, -1f, 0f);
    [SerializeField] private float moveSpeed = 0.005f;
    private Vector3 startPos;

    [SerializeField] private float timeDelay = 10f;

    [SerializeField] private Rigidbody[] partsRbs;
    [SerializeField] private Collider[] partsColls;

    [SerializeField] private float height = -20f;

    private void Start() { Invoke(nameof(StartMove), timeDelay); }

    private void Update()
    {
        if (isMoveStarted)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos + moveOffset, moveSpeed);
            if (transform.position == startPos + moveOffset) Destroy(gameObject);
        }

        if (transform.position.y <= height) Destroy(gameObject);
    }

    private void StartMove()
    {
        startPos = transform.position;
        foreach (var rb in partsRbs) rb.isKinematic = true;
        foreach (var col in partsColls) col.enabled = false;
        isMoveStarted = true;
    }
}
