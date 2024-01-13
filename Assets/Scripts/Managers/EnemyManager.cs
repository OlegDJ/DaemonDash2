using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawners;
    [SerializeField] private GameObject yBotPrefab;
    [SerializeField] private Transform enemyParent;
    private Manager mngr;

    private void Awake()
    {
        mngr = GetComponent<Manager>();
    }

    public void NearestSpawn()
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 curPos = mngr.player.transform.position;

        foreach (Transform t in spawners)
        {
            float dist = Vector3.Distance(t.position, curPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }

        GameObject newYBot = Instantiate(yBotPrefab, tMin.position, Quaternion.identity, enemyParent);
        newYBot.name = yBotPrefab.name;
    }

    public void RandomSpawn()
    {
        GameObject newYBot = Instantiate(yBotPrefab, 
            spawners[Random.Range(0, spawners.Length)].position, Quaternion.identity, enemyParent);
        newYBot.name = yBotPrefab.name;
    }
}
