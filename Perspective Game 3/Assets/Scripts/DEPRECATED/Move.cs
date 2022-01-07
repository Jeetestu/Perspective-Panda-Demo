using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    //end position (in grid coordinates)
    public Vector3Int endGridPos;
    public float moveDuration = 1f;
    public Triggerable trigger;
    //if true the endPos is placed as a displacement rather than a final position
    public bool displace = false;

    private Vector3Int gridPos;
    private bool moved = false;
    private GridManager gridManager;
    private void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        trigger.scripts.AddListener(startMove);
    }

    private void Start()
    {
        gridPos = GridManager.Instance.Grid.getCoord(this.gameObject);
    }



    public void startMove()
    {
        if (!moved)
        {
            moved = true;
            if (displace)
            {
                endGridPos = gridPos + endGridPos;
            }
            //immediately updates the grid location in the grid manager
            GridManager.Instance.Grid.updateBlockPos(this.gameObject);
            StartCoroutine(move());
        }

    }

    IEnumerator move()
    {
        Vector3 startPos = gridManager.getWorldPosition(gridPos);
        Vector3 endPos = gridManager.getWorldPosition(endGridPos);
        float t = 0;
        float progress;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            progress = t / moveDuration;
            transform.position = new Vector3(Mathf.SmoothStep(startPos.x, endPos.x, progress), Mathf.SmoothStep(startPos.y, endPos.y, progress), Mathf.SmoothStep(startPos.z, endPos.z, progress));
            yield return null;
        }
    }
}
