using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpRateRadialFill : MonoBehaviour
{
    public Color fillingColor;
    public Color filledColor;

    public float timeToFill;
    float currFillTime = 0;

    bool filling = false;
    public Image image;

    public RatesExperimentController experimentController;

    // Update is called once per frame
    void Update()
    {
        if (filling)
        {
            currFillTime += Time.deltaTime;

            if (currFillTime >= timeToFill)
                EndFill();
            else
                image.fillAmount = currFillTime / timeToFill;
        }
    }

    public void StartFill()
    {
        image.color = fillingColor;
        currFillTime = 0;
        filling = true;
    }

    void EndFill()
    {
        image.color = filledColor;
        image.fillAmount = 1;
        filling = false;

        experimentController.RunTimerDone();
    }
}
