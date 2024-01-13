using UnityEngine;

[RequireComponent(typeof(Manager))]
public class PopUpTextManager : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab, dodgedTextPrefab, deathTextPrefab;

    public void DisplayDamagePopUpText(int damage, Vector3 textSpawnPosition)
    {
        GameObject damagePopUpText = Instantiate(damageTextPrefab, textSpawnPosition, Quaternion.identity);
        damagePopUpText.GetComponent<PopUpTextController>().SetUp("-" + damage.ToString());
    }

    public void DisplayDodgedPopUpText(Vector3 textSpawnPosition)
    {
        GameObject dodgedPopUpText = Instantiate(dodgedTextPrefab, textSpawnPosition, Quaternion.identity);
        dodgedPopUpText.GetComponent<PopUpTextController>().SetUp("Dodged!");
    }

    public void DisplayDeathPopUpText(Vector3 textSpawnPosition)
    {
        GameObject deathPopUpText = Instantiate(deathTextPrefab, textSpawnPosition, Quaternion.identity);
        deathPopUpText.GetComponent<PopUpTextController>().SetUp("Dead...");
    }
}
