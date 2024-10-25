using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

[System.Serializable]
public class ObjectPool
{
    public GameObject[] objArray;
    public GameObject[] pool;
    public Transform targetParent;

    public int maxPoolSize = 100;
    public int currPoolSize = 0;
    public int queueIndex = 0;

    // Just send in the gameobject in instead?
    public ObjectPool(int maxPoolSize, GameObject[] objArray, Transform targetParent)
    {
        this.maxPoolSize = maxPoolSize;
        pool = new GameObject[maxPoolSize];
        this.objArray = objArray;
        this.targetParent = targetParent;
    }

    public GameObject SpawnObject(Vector3 worldPosition, Quaternion rotation, int objIndex = 0) {

        if (currPoolSize < maxPoolSize) {
            currPoolSize++;
            pool[queueIndex] = Object.Instantiate(objArray[objIndex], worldPosition, rotation, targetParent);
        }
        else {
            pool[queueIndex].transform.SetPositionAndRotation(worldPosition, rotation);
        }
        queueIndex = (queueIndex + 1) % maxPoolSize;
        int prevIndex = queueIndex;
        return pool[prevIndex];
    }

}
