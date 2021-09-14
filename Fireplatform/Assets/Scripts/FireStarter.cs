using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStarter : MonoBehaviour
{
    [Tooltip("The sparkle prefab which starts fires.")]
    public GameObject sparkle;

    // Start is called before the first frame update
    //GGD: Avoid empty start/update methods for performance issues, they are called anyway even if they dont contain code
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Spawn sparkle at mouse on mouseclick
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Instantiate(sparkle, mouseWorldPos, Quaternion.identity, this.transform);
        }
    }

}
