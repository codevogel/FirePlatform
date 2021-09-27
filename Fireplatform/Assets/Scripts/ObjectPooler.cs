using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectPooler<T> : MonoBehaviour
{

    public Queue<T> availableObjects;

    public virtual void AddAndDisable(T obj)
    {
        availableObjects.Enqueue(obj);
    }

    public virtual T GetAndActivate()
    {
        return availableObjects.Dequeue();
    }

}
