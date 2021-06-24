using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlyphLockPiece : MonoBehaviour
{
    public int glyphId;
    public Color correctColor;
    public Color defaultColor;

    GlyphLockController glyphController;

    public enum GlyphLockState
    { 
    locked,
    open
    }

    public GlyphLockState lockState = GlyphLockState.locked; 


    // Start is called before the first frame update
    void Start()
    {
        glyphController = transform.parent.gameObject.GetComponent<GlyphLockController>();
    }


    public void Unlock()
    {
        lockState = GlyphLockState.open;
        GetComponent<Image>().color = correctColor;
    }

    public void ReLock()
    {
        lockState = GlyphLockState.locked;
        GetComponent<Image>().color = defaultColor;
    }
}
