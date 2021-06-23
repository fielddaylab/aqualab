using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProbeHackTextHeader : MonoBehaviour
{
    Text textComponent;
    Animator anim;
    public string defaultText;
    public Color defaultColor;
    public Color greenColor;

    // Start is called before the first frame update
    void Start()
    {
        textComponent = GetComponent<Text>();
        anim = GetComponent<Animator>();

        textComponent.color = defaultColor;
    }

    public void FlashRed()
    {
        anim.enabled = true;
        textComponent.text = "-ERROR-";
        anim.Play("Probename_flashRed");
    }

    public void DoneFlashRed()
    {
        textComponent.text = defaultText;
        anim.enabled = false;
    }

    public void LockOpened()
    {
        textComponent.text = "-ACTIVATION COMPLETE-";
        textComponent.color = greenColor;
        //transform.GetChild(0).GetComponent<SpriteRenderer>().color = greenColor;
    }
}
