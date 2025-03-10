using System.Collections;
using UnityEngine;

public class BoostRespawn : MonoBehaviour
{
    [SerializeField] GameObject itemPrefab; // noss Prefab
    GameObject item;
    bool itemSpawed;
    void Update()
    {
        if (item != null)
        {
            itemSpawed = false;
        }
        if (item != null && !itemSpawed)
        {
            itemSpawed = true;
            StartCoroutine(SpawnItem());
        }
    }

    IEnumerator SpawnItem()
    {
        
        yield return new WaitForSeconds(10);
        item = Instantiate(itemPrefab, transform);
        item.transform.localPosition = new Vector3(0, 1, 0);
    }
}
