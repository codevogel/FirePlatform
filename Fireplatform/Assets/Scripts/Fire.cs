using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{

    public GameObject firePrefab;

    [Tooltip("Amount of time (in seconds) it takes for the fire to spread")]
    public float spreadTime;
    [Tooltip("Distance the fire will try spreading to")]
    public float spreadDistance;

    [Tooltip("Determines whether to draw debug gizmos")]
    public bool drawGizmos;

    /// <summary>
    /// Time at which spreading of fire takes place
    /// </summary>
    private float timeOfSpread;
    
    /// <summary>
    /// Whether this fire has spread
    /// </summary>
    private bool spread = false;

    /// <summary>
    /// The direction in which this fire should spread
    /// </summary>
    public FireDirection Direction { get; set; }

    /// <summary>
    /// Runs at start
    /// </summary>
    private void Start()
    {
        timeOfSpread = Time.time + spreadTime;
    }

    /// <summary>
    /// Run before each frame is drawn
    /// </summary>
    private void Update()
    {
        TrySpreading();
    }

    /// <summary>
    /// Draws gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            Gizmos.color = Color.cyan;

            Vector3 start = this.transform.position;
            Vector3 end = Vector3.zero;
            switch(Direction)
            {
                case FireDirection.BOTH:
                    start = this.transform.position - this.transform.right * spreadDistance;
                    end = this.transform.position + this.transform.right * spreadDistance;
                    break;
                case FireDirection.LEFT:
                    end = this.transform.position - this.transform.right * spreadDistance;
                    break;
                case FireDirection.RIGHT:
                    end = this.transform.position + this.transform.right * spreadDistance;
                    break;


            }
            Gizmos.DrawLine(start, end);
        }
    }

    /// <summary>
    /// Tries spreading this fire in Direction
    /// Only spreads 
    /// </summary>
    private void TrySpreading()
    {
        if (Time.time > timeOfSpread && !spread)
        {
            Spread();
        }
    }

    /// <summary>
    /// Spread the fire in Direction
    /// </summary>
    private void Spread()
    {
        spread = true;
        Vector3 spreadPosition = transform.right * spreadDistance;
        switch(Direction)
        {
            case FireDirection.LEFT:
                SpreadTo(transform.position - spreadPosition, FireDirection.LEFT);
                return;
            case FireDirection.RIGHT:
                SpreadTo(transform.position + spreadPosition, FireDirection.RIGHT);
                return;
            case FireDirection.BOTH:
                SpreadTo(transform.position - spreadPosition, FireDirection.LEFT);
                SpreadTo(transform.position + spreadPosition, FireDirection.RIGHT);
                return;
        }
    }

    /// <summary>
    /// Spread the fire to position pos in direction newDir
    /// </summary>
    /// <param name="newPos">The new position at which to check for spread</param>
    /// <param name="newDir"></param>
    private void SpreadTo(Vector3 newPos, FireDirection newDir)
    {
        // Raycast down from new pos
        RaycastHit2D hit = Physics2D.Raycast(newPos, -this.transform.up);
        if (hit)
        {
            // Find closest position of hit collider to newPos
            Vector3 closestPosition = hit.collider.ClosestPoint(newPos);
            // Instantiate fire at closest point
            // GGD: This will instantiate a lot of fire objects, and instantiate is not good for performance.
            //For now we will leave it like this, but when we integrate this in reggie project, we will use an object
            //pool instead (we already have a class), i.e. an object which reuses the fires which are not in the screen anymore
            //to avoid instantiate so many. You can have a read here: https://docs.google.com/document/d/12Hl_VjBzEIMQ2udz6iJOmjmiuLxPyUCvqHoYcEVPuhs/edit

            Fire newFire = Instantiate(firePrefab, closestPosition, Quaternion.identity, this.transform.parent).GetComponent<Fire>();
            // Rotate in direction of hit normal
            newFire.transform.up = hit.normal;
            // Set new fire's spreading direction
            newFire.Direction = newDir;
        }
    }
    //GGD: Discuss with frank if the fire burns forever, or it has a limited time. 
    //What happens when we passed it and we dont see it anymore? should it be destroyed for performance improvement?
}
