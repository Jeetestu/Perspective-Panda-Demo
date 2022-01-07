using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Triggerable : MonoBehaviour
{
    public float delay = 0f;

    [HideInInspector]
    public UnityEvent scripts;
    // Start is called before the first frame update
    public void activate()
    {
        if (delay > 0f)
            StartCoroutine(activateRoutine());
        else
            scripts.Invoke();
    }

    IEnumerator activateRoutine()
    {
        yield return new WaitForSeconds(delay);
        scripts.Invoke();
    }
}
