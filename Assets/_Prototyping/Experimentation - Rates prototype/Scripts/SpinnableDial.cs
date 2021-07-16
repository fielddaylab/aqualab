using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpinnableDial : MonoBehaviour
{
    public Transform dial;

    public float minVal;
    public float maxVal;

    float currVal;

    float range;

    public Text text;
    public string unit;

    // Start is called before the first frame update
    void Start()
    {
        range = maxVal - minVal;
        SetCurrVal(minVal + range / 2);
    }

    public void MouseIsOverAndDown()
    {
        Vector3 mousePos = Input.mousePosition;
        float angle = Mathf.Atan2(mousePos.y - dial.position.y, mousePos.x - dial.position.x) * Mathf.Rad2Deg;

        //Debug.Log(angle);

        if ((angle < 0) && (angle > -90))
            angle = 0;
        if (angle < -90)
            angle = 180;

        float newValPercent = (1 - (angle / 180));
        SetCurrVal(PercentToVal(newValPercent));
    }

    public void SetCurrVal(float newVal)
    {
        currVal = newVal;

        float percent = ValToPercent(newVal);
        UpdateDial(percent*-180);
        UpdateText();
    }

    float ValToPercent(float val)
    {
        return ((val - minVal) / range);
    }

    float PercentToVal(float percent)
    {
        return ((percent*range) + minVal);
    }

    void UpdateDial(float newAngle)
    {
        dial.eulerAngles = new Vector3(0, 0, newAngle);
    }

    void UpdateText()
    {
        text.text = currVal.ToString("F2") + unit; 
    }
}
