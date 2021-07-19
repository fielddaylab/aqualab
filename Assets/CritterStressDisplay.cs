using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CritterStressDisplay : MonoBehaviour
{
    public GameObject stressObj;
    public Image img;

    public Color stressColor;

    public bool isStressed;

    [Header("Water Chem Stress Values (min safe, max safe)")]
    public float[] tempF;
    public float[] lightLevel;
    public float[] o2;


    public void UpdateStressState(float newTempF, float newLightLevel, float newO2)
    {
        if ((newTempF < tempF[0]) || (newTempF > tempF[1]) || (newLightLevel < lightLevel[0]) || (newLightLevel > lightLevel[1]) || (newO2 < o2[0]) || (newO2 > o2[1]))
            isStressed = true;
        else
            isStressed = false;

        DisplayStressState();
    }

    public void DisplayStressState()
    {
        if (isStressed)
        {
            img.color = stressColor;
            stressObj.SetActive(true);
        }
        else
        {
            img.color = Color.white;
            stressObj.SetActive(false);
        }
    }
}
