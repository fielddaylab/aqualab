using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableSpecies : MonoBehaviour
{
    bool toggled;
    public int id;
    public GameObject checkObj;
    public RatesExperimentController experimentController;

    private void Start()
    {
        toggled = false;
        checkObj.SetActive(toggled);
    }

    public void OnClicked()
    {
        toggled = !toggled;
        checkObj.SetActive(toggled);
        experimentController.SetSpeciesSelection(id, toggled);
    }
}
