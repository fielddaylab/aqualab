using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpRateSwitch : MonoBehaviour
{
    public Color enabledColor;
    public Color disabledColor;
    public RatesExperimentController.switchType switchType;
    public RatesExperimentController experimentController;

    Image image;

    bool isEnabled = false;

    private void Start()
    {
        image = GetComponent<Image>();

        image.color = disabledColor;
    }

    public void OnSelected()
    {
        isEnabled = !isEnabled;

        if (isEnabled)
            image.color = enabledColor;
        else
            image.color = disabledColor;

        //flip switch sprite
        transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, 1);

        experimentController.SetSwitch(switchType, isEnabled);
    }


}
