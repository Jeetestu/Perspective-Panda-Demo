using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPoint : MonoBehaviour
{
    public delegate void MovementComplete();
    public event MovementComplete OnMovementComplete;

    public float speed = 2f;
    public Vector3 target;
    public bool destroyOnComplete = true;

    public void startMovement (Vector3 start, Vector3 target, bool freeFromParent = false, bool destroyOnComplete = true, float minTimeToComplete = 0f)
    {
        if (freeFromParent)
            transform.parent = null;
        transform.position = start;
        this.target = target;
        this.destroyOnComplete = destroyOnComplete;
        timeTracker = 0f;
        this.minTimeToComplete = minTimeToComplete;
        StartCoroutine(movement());
    }


    float minTimeToComplete;
    float timeTracker;

    IEnumerator movement()
    {

        while (transform.position != target)
        {
            timeTracker += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }

        while (timeTracker < minTimeToComplete)
        {
            timeTracker += Time.deltaTime;
            yield return null;
        }

        OnMovementComplete?.Invoke();

        if (destroyOnComplete)
            DestroyImmediate(this.gameObject);
    }
}
