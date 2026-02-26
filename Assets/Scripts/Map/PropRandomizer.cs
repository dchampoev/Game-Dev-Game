using System.Collections.Generic;
using UnityEngine;

public class PropRandomizer : MonoBehaviour
{
    public List<GameObject> propSpawnPoints;
    public List<GameObject> propPrefabs;

    void Start()
    {
        SpawnProps();
    }

    void SpawnProps()
    {
        foreach (GameObject spawnPoint in propSpawnPoints)
        {
            int randomIndex = Random.Range(0, propPrefabs.Count);
            GameObject prop = Instantiate(propPrefabs[randomIndex], spawnPoint.transform.position, Quaternion.identity);
            prop.transform.parent = spawnPoint.transform;
        }
    }
}
