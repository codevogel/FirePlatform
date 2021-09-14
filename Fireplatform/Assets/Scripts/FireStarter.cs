using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireStarter : MonoBehaviour
{
    [Tooltip("The sparkle prefab which starts fires.")]
    public GameObject sparkle;

    private List<Fire> activeFires;

    // Start is called before the first frame update
    void Start()
    {
        activeFires = new List<Fire>();
    }

    // Update is called once per frame
    void Update()
    {
        ConnectActiveFires();
        // Spawn sparkle at mouse on mouseclick
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            StartSparkle(mouseWorldPos);
        }
    }

    /// <summary>
    /// Starts a sparkle at a given world position
    /// </summary>
    /// <param name="position">The world position at which a sparkle starts.</param>
    public void StartSparkle(Vector2 position)
    {
        Instantiate(sparkle, position, Quaternion.identity, this.transform);
    }
    
    /// <summary>
    /// Checks if active fires have looped around platforms and stops them
    /// from trying to spread further.
    /// </summary>
    public void ConnectActiveFires()
    {
        // If active fires exist
        if (activeFires.Count > 0)
        {
            List<Fire> toBeRemoved = new List<Fire>();
            // For each parent fire
            foreach (Fire parentFire in activeFires)
            {
                // Check whether leftmost and rightmost fire meet
                if (Vector2.Distance(parentFire.LeftLeafFire.transform.position, parentFire.RightLeafFire.transform.position) < parentFire.LeftLeafFire.spreadDistance)
                {
                    // If they do, platform is engulved in fire, stop spreading.
                    parentFire.LeftLeafFire.MarkSpread();
                    parentFire.RightLeafFire.MarkSpread();
                    // Add this parentfire to be removed from the list
                    toBeRemoved.Add(parentFire);
                }
            }
            // Remove completed fires from the list of active fires.
            if (toBeRemoved.Count > 0)
            {
                foreach (Fire parentFire in toBeRemoved)
                {
                    activeFires.Remove(parentFire);
                }
            }
        }
    }

    /// <summary>
    /// Adds an active parent fire to the list of active fires
    /// </summary>
    /// <param name="parentFire"></param>
    public void AddActiveFire(Fire parentFire)
    {
        activeFires.Add(parentFire);
    }

}
