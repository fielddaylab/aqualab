using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Aqua;
using ProtoAqua.Observation;
using UnityEngine.UI;
using System;

public class ProbeHackSceneManager : MonoBehaviour
{
    public GameObject probeHackParent;
    public GameObject[] scannableGlyphs;

    ScannableRegion probeScannableRegion;

    public BaseInputLayer m_Input;

    // Start is called before the first frame update
    void Awake()
    {
        //Debug.Log("Is m_Input null: "+(m_Input == null).ToString());
        m_Input = BaseInputLayer.Find(this);
    }

    public void LoadProbeHack(ScannableRegion probeScanRegion)
    {
        probeScannableRegion = probeScanRegion;

        EnableRaycast();
        probeHackParent.SetActive(true);

        m_Input.PushPriority();
    }

    public void UnloadProbeHack(bool wasUnlocked)
    {
        m_Input.PopPriority();
        probeHackParent.SetActive(false);

        if (wasUnlocked)
        {
            probeScannableRegion.completedHackMinigame = true;

            DisableScannableGlyphs();
            //probeScannableRegion.CompleteScan();     
        }
    }

    private void DisableScannableGlyphs()
    {
        foreach (GameObject go in scannableGlyphs)
            go.SetActive(false);
    }

    private void OnMouseDown()
    {
        LoadProbeHack(null);
    }

    private void EnableRaycast()
    {
        GetComponent<GraphicRaycaster>().enabled = true;
    }
}
