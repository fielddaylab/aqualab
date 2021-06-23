using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Aqua;
using ProtoAqua.Observation;

public class ProbeHackSceneManager : MonoBehaviour
{
    public GameObject probeHackParent;
    public GameObject playerObj;

    ScannableRegion probeScannableRegion;

    public PlayerROVScanner scanner;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadProbeHack(ScannableRegion probeScanRegion)
    {
        probeScannableRegion = probeScanRegion;
        probeHackParent.SetActive(true);
        playerObj.SetActive(false);

        //SceneManager.LoadScene(probehackScene, LoadSceneMode.Additive);
    }

    public void UnloadProbeHack(bool wasUnlocked)
    {
        if (wasUnlocked)
        {
            probeHackParent.SetActive(false);
            playerObj.SetActive(true);

            probeScannableRegion.completedHackMinigame = true;
            //probeScannableRegion.CompleteScan();     
        }
    }

    private void OnMouseDown()
    {
        LoadProbeHack(null);
    }
}
