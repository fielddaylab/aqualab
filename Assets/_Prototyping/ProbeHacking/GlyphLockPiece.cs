using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlyphLockPiece : MonoBehaviour
{
    public int glyphId;
    public Color correctColor;
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CorrectPieceMatched()
    {
        lockState = GlyphLockState.open;
        GetComponent<SpriteRenderer>().color = correctColor;
        glyphController.CheckLocks();
    }
}
