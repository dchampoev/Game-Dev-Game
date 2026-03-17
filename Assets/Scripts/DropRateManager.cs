using System.Collections.Generic;
using UnityEngine;

public class DropRateManager : MonoBehaviour
{
    [System.Serializable]
    public class Drops
    {
        public string dropName;
        public GameObject dropPrefab;
        public float dropRate;
    }

    public List<Drops> drops;

    void OnDestroy()
    {
        if (!gameObject.scene.isLoaded)
        {
            return;
        }

        float randomNumber = Random.Range(0f, 100f);
        List<Drops> possibleDrops = new List<Drops>();

        foreach (Drops drop in drops)
        {
            if (randomNumber <= drop.dropRate)
            {
                possibleDrops.Add(drop);
            }
        }
        if (possibleDrops.Count > 0)
        {
            Drops drop = possibleDrops[Random.Range(0, possibleDrops.Count)];
            Instantiate(drop.dropPrefab, transform.position, Quaternion.identity);
        }
    }
}
