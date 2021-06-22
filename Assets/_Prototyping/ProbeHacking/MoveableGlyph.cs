using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableGlyph : MonoBehaviour
{
    public GameObject holderObj;
    public int glyphId;

    enum MoveState
    { 
    inHolder,
    beingClicked,
    inSolution
    }

    MoveState state = MoveState.inHolder;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (state == MoveState.beingClicked)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            transform.position = mousePos;
        }
    }

    private void OnMouseDown()
    {
        if (state == MoveState.inHolder)
            state = MoveState.beingClicked;
    }

    private void OnMouseUp()
    {
        Vector3 boxSize = new Vector3(0.125f, 0.125f, 0.1f);
        Collider[] hitCollider = Physics.OverlapBox(transform.position, boxSize);
        foreach (Collider col in hitCollider)
        {
            if (col.tag != "GlyphLock")
                continue;

            if (col.GetComponent<GlyphLockPiece>().glyphId == glyphId)
            {
                //Found a match
                col.GetComponent<GlyphLockPiece>().CorrectPieceMatched();
                Destroy(gameObject);
            }
        }
        BackToHolder();
    }

    //Returns glyph back to staring position
    public void BackToHolder()
    {
        state = MoveState.inHolder;
        transform.position = holderObj.transform.position;
    }
}
