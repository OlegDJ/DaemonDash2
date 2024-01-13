using UnityEngine;
using Random = UnityEngine.Random;

public class AttackTrigger : MonoBehaviour
{
    [SerializeField] private int minDamage, maxDamage;
    [SerializeField] private bool canBeDodged;
    private bool willBeDodged;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Block Trigger")) willBeDodged = canBeDodged;

        if (other.TryGetComponent<IDamageable>(out var controller))
        {
            controller.GetAttacked(Random.Range(minDamage, maxDamage), willBeDodged);
            willBeDodged = false;
        }
    }
}
