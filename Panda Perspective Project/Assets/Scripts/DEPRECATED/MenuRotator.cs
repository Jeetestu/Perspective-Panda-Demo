using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuRotator : MonoBehaviour
{
    [System.Serializable]
    public struct SubmenuData
    {
        public Vector3 viewAngle;
        public GameObject menuObject;
    }
    public SubmenuData[] subMenus;
    public float speed;

    private RectTransform rect;

    public GameObject activeSubmenu;


    Quaternion targetRotation;
    bool finishedTransition = true;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        rect.localRotation = Quaternion.Euler(getSubmenuData(activeSubmenu).viewAngle);
        deactiveAllOtherSubmenus();
    }


    private void Update()
    {
        if (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * speed);
        }
        //this statement runs when the transition has finished
        else if (!finishedTransition)
        {
            finishedTransition = true;
            deactiveAllOtherSubmenus();
        }
    }

    private void deactiveAllOtherSubmenus()
    {
        foreach (SubmenuData sd in subMenus)
            if (sd.menuObject != activeSubmenu)
                sd.menuObject.SetActive(false);
    }

    private void flattenUI (Transform parent)
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            children.Add(parent.GetChild(i));
            for (int j = 0; j < parent.GetChild(i).childCount; j++)
                children.Add(parent.GetChild(i).GetChild(j));
        }

        foreach (Transform child in children)
            child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, 0f);
    }

    private void popUI (Transform parent)
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            children.Add(parent.GetChild(i));
            for (int j = 0; j < parent.GetChild(i).childCount; j++)
                children.Add(parent.GetChild(i).GetChild(j));
        }

        foreach (Transform child in children)
            child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, Random.Range(0, -1000f));
    }

    public void rotateToSubmenu(GameObject submenu)
    {
        foreach (SubmenuData sd in subMenus)
                sd.menuObject.SetActive(true);
        popUI(activeSubmenu.transform);
        activeSubmenu.transform.SetAsLastSibling();
        targetRotation = Quaternion.Euler(getSubmenuData(submenu).viewAngle);
        flattenUI(submenu.transform);
        activeSubmenu = submenu;
        finishedTransition = false;
    }

    private SubmenuData getSubmenuData(GameObject submenu)
    {
        foreach (SubmenuData si in subMenus)
            if (si.menuObject == submenu)
                return si;
        Debug.LogError("Could not find submenu item for submenu: " + submenu.name);
        return subMenus[0];
    }
    

}
