using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPooler : ObjectPooler<GameObject>
{

    private Queue<GameObject> availableObjects;

    public override void AddAndDisable(GameObject obj)
    {
        base.AddAndDisable(obj);
        obj.SetActive(false);
    }

    public override GameObject GetAndActivate()
    {
        GameObject obj = base.GetAndActivate();
        obj.SetActive(true);
        return obj;
    }
}

