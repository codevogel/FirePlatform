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
    [Tooltip("Sets the distance between the raycast origin and the potential new fire location")]
    public float rayCastMagnitude = 5;

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
    /// The firestarter object (there should be just one instance of this)
    /// </summary>
    public FireStarter FireStarter { private get; set; }
    /// <summary>
    /// Gets or sets this fire's parent fire
    /// </summary>
    public Fire ParentFire { get; private set; }

    /// <summary>
    /// Gets or sets this fire's left leaf fire\
    /// The left leaf fire is the currently right-most fire instance started by the parentfire.
    /// </summary>
    public Fire LeftLeafFire { get; private set; }

    /// <summary>
    /// Gets or sets this fire's right leaf fire\
    /// The right leaf fire is the currently right-most fire instance started by the parentfire.
    /// </summary>
    public Fire RightLeafFire { get; private set; }

    /// <summary>
    /// The trigger for this fire
    /// </summary>
    public BoxCollider2D Trigger { get; set; }

    //TODO: Remove debug vars
    private Vector2 circleCastGizmoOrigin;
    private float circleCastGizmoTimeOfDeath;
    private float circleCastGizmoLifetime = 3f;

    /// <summary>
    /// Runs at start
    /// </summary>
    private void Start()
    {
        timeOfSpread = Time.time + spreadTime;
        Trigger = GetComponent<BoxCollider2D>();
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
            // Draw line from current fire to the position it will try to start a new fire at.
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

            // Draw circlecast gizmo
            Gizmos.color = Color.magenta;
            if (circleCastGizmoOrigin != Vector2.zero)
            {
                if (Time.time < circleCastGizmoTimeOfDeath)
                {
                    Gizmos.DrawWireSphere(circleCastGizmoOrigin, 1);
                    return;
                }
                circleCastGizmoOrigin = Vector2.zero;
            }

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
        MarkSpread();
        Vector3 spreadPosition = transform.right * spreadDistance;
        switch(Direction)
        {
            case FireDirection.LEFT:
                // Spread left
                Fire leftLeafFire = SpreadTo(transform.position - spreadPosition, FireDirection.LEFT);
                // If not a parent fire
                if (ParentFire != null)
                {
                    // Set new leaf fire's parent fire
                    leftLeafFire.ParentFire = ParentFire;
                    // Update the parent's left leaf fire.
                    ParentFire.LeftLeafFire = leftLeafFire;
                }
                return;
            case FireDirection.RIGHT:
                // Spread right
                Fire rightLeafFire = SpreadTo(transform.position + spreadPosition, FireDirection.RIGHT);
                if (ParentFire != null)
                {
                    // Set right leaf fire's parent fire
                    rightLeafFire.ParentFire = ParentFire;
                    // Update the parent's right leaf fire.
                    ParentFire.RightLeafFire = rightLeafFire;
                }
                return;
            case FireDirection.BOTH:
                // This is a parent fire, so no parent exists for this fire.
                ParentFire = null;
                // Spread left and right
                LeftLeafFire = SpreadTo(transform.position - spreadPosition, FireDirection.LEFT);
                RightLeafFire = SpreadTo(transform.position + spreadPosition, FireDirection.RIGHT);
                // Set parent of new fires to this
                LeftLeafFire.ParentFire = this;
                RightLeafFire.ParentFire = this;
                // Add this parent fire to the FireStarter active list
                FireStarter.AddActiveFire(this);
                return;
        }
    }

    /// <summary>
    /// Marks this fire as already spread.
    /// </summary>
    public void MarkSpread()
    {
        spread = true;
    }

    /// <summary>
    /// Spread the fire to position pos in direction newDir
    /// </summary>
    /// <param name="potentialLocation">The new position at which to check for spread</param>
    /// <param name="newDir"></param>
    private Fire SpreadTo(Vector3 potentialLocation, FireDirection newDir)
    {
        // Determine raycast direction and magnitude
        Vector3 raycastVector = this.transform.up * rayCastMagnitude;

        // Cast from 'above' potential location to prevent getting stuck in collider.
        RaycastHit2D hit = Physics2D.Raycast(potentialLocation + raycastVector, -this.transform.up, 1000f, LayerMask.GetMask("Ground"));
        if (hit)
        {
            //TODO: Remove debug tool
            if (drawGizmos)
            {
                Debug.DrawRay(potentialLocation + raycastVector, -raycastVector, Color.magenta, 1);
            }
            // Find closest position of hit collider to newPos
             Vector3 closestPosition = hit.collider.ClosestPoint(potentialLocation);
            // Instantiate fire at closest point
            Fire newFire = Instantiate(firePrefab, closestPosition, Quaternion.identity, this.transform.parent).GetComponent<Fire>();
            newFire.FireStarter = FireStarter;
            // Rotate in direction of hit normal
            newFire.transform.up = hit.normal;
            // Set new fire's spreading direction
            newFire.Direction = newDir;
            return newFire;
        }
        else
        {
            // If nothing was found, try circlecasting at original potential location, as platform may have had a <90 degree corner.
            hit = Physics2D.CircleCast(potentialLocation, 5, Vector2.up, 0, LayerMask.GetMask("Ground"));
            // TODO: remove debug tool
            if (drawGizmos)
            {
                circleCastGizmoOrigin = potentialLocation;
                circleCastGizmoTimeOfDeath = Time.time + circleCastGizmoLifetime;
            }
            if (hit)
            {
                Vector3 closestPosition = hit.transform.GetComponent<PolygonCollider2D>().ClosestPoint(this.transform.position);
                // Instantiate fire at closest point
                Fire newFire = Instantiate(firePrefab, closestPosition, Quaternion.identity, this.transform.parent).GetComponent<Fire>();
                // Rotate in direction of hit normal
                newFire.transform.up = hit.normal;
                // Set new fire's spreading direction
                newFire.Direction = newDir;
                return newFire;
            }
            return null;
        }
    }
}
