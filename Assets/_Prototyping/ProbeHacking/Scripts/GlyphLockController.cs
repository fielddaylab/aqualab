using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GlyphLockController : MonoBehaviour
{
    public List<GameObject> glyphLocks = new List<GameObject>();
    List<GameObject> enteredGlyphs = new List<GameObject>();

    public Color allUnlocked;

    public BoxCollider2D checkmarkCollider;

    public GameObject checkObj;
    public ProbeHackTextHeader textHeader;

    bool unlocked = false;

    int currentGlyphLockIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        int i = 0;

        while (i < transform.childCount)
        {
            glyphLocks.Add(transform.GetChild(i).gameObject);
            i++;
        }

        transform.GetChild(0).GetComponent<Animator>().enabled = true;
    }

    /*public void CheckLocks()
    {
        foreach(GameObject glyphLock in glyphLocks)
        {
            if (glyphLock.GetComponent<GlyphLockPiece>().lockState == GlyphLockPiece.GlyphLockState.locked)
                return;   
        }

        //If code gets here, lock complete!
        OpenLock();
    }*/
    
    public void SelectGlyph(GameObject glyphObj)
    {
        if (unlocked == true)
            return;

        GlyphLockPiece currentLockedGlyph = glyphLocks[currentGlyphLockIndex].GetComponent<GlyphLockPiece>();
        if (glyphObj.GetComponent<MoveableGlyph>().glyphId == currentLockedGlyph.glyphId)
        {
            //Correct glyph selected

            //Add moveableGlyph to array of entered glyphs + disable
            enteredGlyphs.Add(glyphObj);
            glyphObj.SetActive(false);

            //Stop glow anim and change color of current glyph
            glyphLocks[currentGlyphLockIndex].GetComponent<Animator>().enabled = false;
            currentLockedGlyph.Unlock();

            //Set next glyph as current
            currentGlyphLockIndex++;

            //Check if pattern completed
            if (currentGlyphLockIndex >= glyphLocks.Count)
                OpenLock();
            else
            {
                //Start glow on new glyph
                glyphLocks[currentGlyphLockIndex].GetComponent<Animator>().enabled = true;
                glyphLocks[currentGlyphLockIndex].GetComponent<Animator>().Play("GlyphLockGlow", 0, 0f);
            }
        }
        else
        {
            //Wrong glyph selected
            ResetLock();
        }
    }

    void ResetLock()
    {
        //Make header and outline flash
        textHeader.FlashRed();

        //Re-enable all entered glyphs
        foreach (GameObject enteredGlyph in enteredGlyphs)
            enteredGlyph.SetActive(true);

        //Set unlocked glyphs back to default color
        foreach (GameObject glyphLock in glyphLocks)
            glyphLock.GetComponent<GlyphLockPiece>().ReLock();

        glyphLocks[currentGlyphLockIndex].GetComponent<Animator>().enabled = false;

        //Reset current GlyphLock index + play animation
        currentGlyphLockIndex = 0;
        glyphLocks[0].GetComponent<Animator>().enabled = true;
        glyphLocks[0].GetComponent<Animator>().Play("GlyphLockGlow", 0, 0f);
    }

    void OpenLock()
    {
        unlocked = true;

        //Change text and color of header
        textHeader.LockOpened();

        //Enable and animate check mark
        checkmarkCollider.enabled = true;
        checkObj.GetComponent<Animator>().Play("CheckFlash");

        //Set all glyphs in lock to green
        foreach (GameObject glyphLock in glyphLocks)
        {
            glyphLock.GetComponent<SpriteRenderer>().color = allUnlocked;
        }
    }

    //Pressing X or Checkmark buttons close scene
    private void OnMouseDown()
    {
        if (GameObject.Find("ProbeHackSceneManager") != null)
            GameObject.Find("ProbeHackSceneManager").GetComponent<ProbeHackSceneManager>().UnloadProbeHack(unlocked);
    }
}
