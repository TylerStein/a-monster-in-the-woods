using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPointer : MonoBehaviour
{
    public Transform target;

    // Update is called once per frame
    void Update()
    {
        if (target) {
            Vector3 adjustedTarget = target.position;
            adjustedTarget.y = transform.position.y;

            transform.LookAt(adjustedTarget);
        }
    }
}
