﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RatesExperimentController : MonoBehaviour
{
    //GiantKelp, Urchin, Otter, BullKelp
    bool[] selectedSpecies = { false, false, false, false };
    public Sprite[] critterSprites;
    public bool autoFeederEnabled = false;
    public bool waterStabilizerEnabled = false;

    [Header("Switching to Measurement Tank")]
    public GameObject addSpeciesCanvas;
    public GameObject measurementTankCanvas;
    public GameObject measurementTankCritters;

    [Header("RatesProgress")]
    public GameObject progressBacking;
    public ExpRateRadialFill radialFill;
    public GameObject experimentDone;

    [Header("Experiment Done Text")]
    public Text reproText;
    public Text eatText;
    public Text waterText;

    [Header("Experiment Done Sprites + Objects")]
    public GameObject reproRateObj;
    public GameObject eatRateObj;
    public GameObject waterRateObj;

    public Image reproCritter;
    public Image predatorCritter;
    public Image preyCritter;
    public Image waterCritter;

    public enum switchType
    {
        AutoFeeder,
        WaterStabilizer
    }

    public void SetSpeciesSelection(int index, bool selected)
    {
        selectedSpecies[index] = selected;
    }

    public void SetSwitch(switchType switchId, bool enabledState)
    {
        if (switchId == switchType.AutoFeeder)
            autoFeederEnabled = enabledState;
        else
            waterStabilizerEnabled = enabledState;
    }

    public void ToMeasurementTank()
    {
        addSpeciesCanvas.SetActive(false);
        measurementTankCanvas.SetActive(true);

        for (int i = 0; i < measurementTankCritters.transform.childCount; i++)
        {
            measurementTankCritters.transform.GetChild(i).gameObject.SetActive(selectedSpecies[i]);
        }
    }

    public void ToAddSpecies()
    {
        measurementTankCanvas.SetActive(false);
        addSpeciesCanvas.SetActive(true);
    }

    public void StartRun()
    {
        progressBacking.SetActive(true);
        experimentDone.SetActive(false);
        radialFill.StartFill();
    }

    public void CloseRun()
    {
        progressBacking.SetActive(false);
    }

    public void RunTimerDone()
    {
        experimentDone.SetActive(true);

        reproRateObj.SetActive(false);
        eatRateObj.SetActive(false);
        waterRateObj.SetActive(false);

        SetDoneText();
    }

    public void SetDoneText()
    {
        List<int> activeSpeciesIndex = new List<int>(); //array that tracks which species are active currently

        //Get number of Species
        int numSpecies = 0;
        for (int i = 0; i < selectedSpecies.Length; i++)
        {
            if (selectedSpecies[i])
            {
                activeSpeciesIndex.Add(i);
                numSpecies++;
            }
        }

        switch (numSpecies)
        {
            case (0):
                {
                    reproText.text = "NO RESULTS\nThere is nothing here to observe";
                    eatText.text = "NO RESULTS\nThere is nothing here to observe";
                    waterText.text = "NO RESULTS\nThere is nothing here to observe";
                    break;
                }

            case (1):
                {
                    //Repro Text
                    if (autoFeederEnabled)
                    {
                        reproText.text = "NO RESULTS\nCritter does not reproduce";

                        //Repro Rate
                        CheckDisplayReproRate(activeSpeciesIndex);
                    }
                    else
                    {
                        reproText.text = "NO RESULTS\nFood required to accurately measure reproduction";
                    }

                    //Eat text
                    eatText.text = "NO RESULTS\nMultiple species required for predation measurement";

                    //Water text
                    if (waterStabilizerEnabled)
                    {
                        waterText.text = "NO RESULTS\nStabilizer active";
                    }
                    else
                    {
                        if (autoFeederEnabled)
                        {
                            waterText.text = "NO RESULTS\nNo changes in water chemistry found";

                            //Prod/Consume Rate
                            CheckDisplayWaterRate(activeSpeciesIndex);
                        }
                        else
                        {
                            waterText.text = "NO RESULTS\nFood required to accurately measure water chemistry changes";
                        }
                    }

                    break;
                }

            case(2): //2 species
                {
                    //Repro Text
                    reproText.text = "NO RESULTS\nToo many species to accurately measure reproduction";

                    //Eat Text
                    if (autoFeederEnabled)
                    {
                        eatText.text = "NO RESULTS\nAutofeeder interference detected";
                    }
                    else
                    {
                        eatText.text = "NO RESULTS\nNo Predation detected";

                        //Eat Rate
                        CheckDisplayEatRate(activeSpeciesIndex);
                    }

                    //Water Text
                    waterText.text = "NO RESULTS\nToo many species to accurately measure water changes";

                    break;
                }

            default: //3+ species
                {
                    //Repro Text
                    reproText.text = "NO RESULTS\nToo many species to accurately measure reproduction";

                    //Eat Text
                    eatText.text = "NO RESULTS\nToo many species to accurately measure predation";

                    //Water Text
                    waterText.text = "NO RESULTS\nToo many species to accurately measure water changes";

                    break;
                }
        }
    }


    void CheckDisplayReproRate(List<int> activeSpecies)
    {
        //Assume all critters reproduce
        reproRateObj.SetActive(true);
        reproCritter.sprite = critterSprites[activeSpecies[0]];
    }

    void CheckDisplayEatRate(List<int> activeSpecies)
    {
        if (!selectedSpecies[1])
            return;

        eatRateObj.SetActive(true);

        //Urchin Eats GiantKelp
        if (selectedSpecies[0])
        {
            preyCritter.sprite = critterSprites[0];
            predatorCritter.sprite = critterSprites[1];
        }

        //Urchin Eats BullKelp
        if (selectedSpecies[3])
        {
            preyCritter.sprite = critterSprites[3];
            predatorCritter.sprite = critterSprites[1];
        }

        //Otter Eats Urchin
        if (selectedSpecies[2])
        {
            preyCritter.sprite = critterSprites[1];
            predatorCritter.sprite = critterSprites[2];
        }
    }

    void CheckDisplayWaterRate(List<int> activeSpecies)
    {
        //Assume all critters change water chemistry
        waterRateObj.SetActive(true);
        waterCritter.sprite = critterSprites[activeSpecies[0]];
    }
}