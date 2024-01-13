using System.Collections;
using UnityEngine;

public class HealingStation : MonoBehaviour
{
    [SerializeField] private int heal = 20;
    [SerializeField] private float healInterval = 5f;
    private bool isHealing;
    private PlayerController playerController;
    private ParticleSystem partSys;

    private void Awake()
    {
        partSys = GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var controller))
        {
            if (playerController == null) playerController = controller;
            isHealing = true;
            var shape = partSys.shape;
            shape.arcMode = ParticleSystemShapeMultiModeValue.Loop;
            StartCoroutine(HealRoutine());
        }
    }

    private IEnumerator HealRoutine()
    {
        while (isHealing)
        {
            yield return new WaitForSeconds(healInterval);
            if (playerController != null) playerController.Heal(heal);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isHealing = false;
            var shape = partSys.shape;
            shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
        }
    }
}
