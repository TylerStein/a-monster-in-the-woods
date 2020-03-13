using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableController : MonoBehaviour
{
    [SerializeField] Transform[] itemSpawns = new Transform[6];
    [SerializeField] GameObject runePrefab;

    void Start()
    {
        foreach(Transform spawn in itemSpawns)
        {
            Instantiate(runePrefab, spawn.position, spawn.rotation);
        }
    }

    public void BadEnd()
    {

    }

    public void GoodEnd()
    {

    }
}
