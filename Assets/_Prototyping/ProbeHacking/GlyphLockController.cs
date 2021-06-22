using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GlyphLockController : MonoBehaviour
{
    public List<GameObject> glyphLocks = new List<GameObject>();
    public Color allUnlocked;

    public GameObject checkObj;

    // Start is called before the first frame update
    void Start()
    {
        int i = transform.childCount;
        i--;

        while (i >= 0)
        {
            glyphLocks.Add(transform.GetChild(i).gameObject);
            i--;
        }
    }

    public void CheckLocks()
    {
        foreach(GameObject glyphLock in glyphLocks)
        {
            if (glyphLock.GetComponent<GlyphLockPiece>().lockState == GlyphLockPiece.GlyphLockState.locked)
                return;   
        }

        //If code gets here, lock complete!
        OpenLock();
    }

    void OpenLock()
    {
        GetComponent<BoxCollider2D>().enabled = true;
        checkObj.GetComponent<Animator>().Play("CheckFlash");

        foreach (GameObject glyphLock in glyphLocks)
        {
            glyphLock.GetComponent<SpriteRenderer>().color = allUnlocked;
        }
    }
    private void OnMouseDown()
    {
        GameObject.Find("ProbeHackSceneManager").GetComponent<ProbeHackSceneManager>().UnloadProbeHack();
    }
}
