using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour  // make class abstract to inherit to child clsss(player, enemy)
{
    public float moveTime = 0.1f;

    // This layer has an open space to move to, and it's a place to check if there's a collision when you try to get there.
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    // used to calculate movement more efficiently.
    private float inverseMoveTime;

    // child class can override this function.
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        // By storing the reciprocal of 'moveTime', you can use efficient multiplication rather than division.
        inverseMoveTime = 1f / moveTime;

    }

    // out means get input by reference(to return more than two variables).
    protected bool Move(int xDir, int yDir, out RaycastHit2D hit) 
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        // Avoid hitting its own collider when using Raycast
        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        // The space checked by Linecast is open and can be moved to it.
        if (hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            return true;    // can move
        }

        return false;   // failed to move.
    }

    // move the units from one place to another. end(Vector3) is destination.
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        // Calculate the remaining distance to move
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;  // Magnitude : vector length. sqrMagnitude : squared vector length. (sqrMagnitude is faster in calculation than Magnitude.)

        while (sqrRemainingDistance > float.Epsilon)   // Epsilon is a extremely small number close to zero.
        {
            // In the loop, based on moveTime, it finds a new position close to the end appropriately.
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;  // Wait for the next frame before updating the loop.
        }
    }

    // Generic input T is used for when unit is blocked, points to the component type the unit will react to.
    // (Ex). If applied to an enemy the opponent will be the player, if applied to the player the opponent will be walls.
    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null)
            return;

        T hitComponent = hit.transform.GetComponent<T>();

        // The moving obeject is blocked, and collide with an interactable object.
        if (!canMove && hitComponent != null)
            OnCantMove(hitComponent);
        
    }

    protected abstract void OnCantMove<T>(T component)
        where T : Component;

}
