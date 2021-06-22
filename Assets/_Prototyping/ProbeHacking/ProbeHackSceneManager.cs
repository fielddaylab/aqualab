using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProbeHackSceneManager : MonoBehaviour
{
    public string probehackScene;
    public GameObject parentOfEverything;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadProbeHack(string sceneToLoad)
    {
        probehackScene = sceneToLoad;
        parentOfEverything.SetActive(false);
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
    }

    public void UnloadProbeHack()
    {
        parentOfEverything.SetActive(true);
        SceneManager.UnloadSceneAsync(probehackScene);
        Destroy(gameObject);
    }
    private void OnMouseDown()
    {
        LoadProbeHack(probehackScene);
    }
}
