using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectEnvironmentHandler : MonoBehaviour
{
    public GameObject listObj;

    public GameObject triangleObj;

    bool listOpen;

    // Start is called before the first frame update
    void Start()
    {
        listOpen = false;
    }

    public void OnClicked()
    {
        if (listOpen)
            CloseList();
        else
            OpenList();
    }

    public void OpenList()
    {
        triangleObj.transform.eulerAngles = new Vector3(0, 0, 90);
        listObj.SetActive(true);
        listOpen = true;
    }

    public void CloseList()
    {
        triangleObj.transform.eulerAngles = new Vector3(0, 0, -90);
        listObj.SetActive(false);
        listOpen = false;
    }
}
