using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableSite : MonoBehaviour
{
    public float temp;
    public float lightDepth;
    public float o2;

    public SpinnableDial tempDial;
    public SpinnableDial lightDial;
    public SpinnableDial o2Dial;

    public SelectEnvironmentHandler envHandler;

    public void OnSelected()
    {
        if (tempDial.InC())
            tempDial.SetCurrVal(tempDial.FToC(temp));
        else
            tempDial.SetCurrVal(temp);

        lightDial.SetCurrVal(lightDepth);
        o2Dial.SetCurrVal(o2);

        envHandler.CloseList();
    }
}
