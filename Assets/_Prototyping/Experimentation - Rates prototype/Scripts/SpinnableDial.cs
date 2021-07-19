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

    public RatesExperimentController expController;

    public int waterChemIndex; //0 = temp, 1 = light, 2 = o2

    bool inC;

    // Start is called before the first frame update
    void Start()
    {
        inC = false;

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

        if (inC)
            expController.waterChemValues[waterChemIndex] = CToF(currVal);
        else
            expController.waterChemValues[waterChemIndex] = currVal;
        expController.UpdateWaterChemistry();
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
        text.text = currVal.ToString("F1") + unit; 
    }

    public void SwapUnits()
    {
        if (inC)
            SwapToF();
        else
            SwapToC();

        inC = !inC;

        range = range = maxVal - minVal;

        SetCurrVal(currVal);
        UpdateText();
    }

    void SwapToF()
    {
        unit = " °F";

        minVal = CToF(minVal);
        maxVal = CToF(maxVal);

        currVal = CToF(currVal);
    }

    void SwapToC()
    {
        unit = " °C";

        minVal = FToC(minVal);
        maxVal = FToC(maxVal);

        currVal = FToC(currVal);
    }

    public float FToC(float degF)
    {
        return ((5f / 9f) * (degF - 32f));
    }

    public float CToF(float degC)
    {
        return (((9f / 5f) * degC) + 32f);
    }

    public bool InC()
    {
        return inC;
    }
}
