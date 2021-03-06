using BeauUtil;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aqua
{
    public class FastBootController : MonoBehaviour, ISceneLoadHandler
    {
        public void OnSceneLoad(SceneBinding inScene, object inContext)
        {
            int buildIdx = SceneHelper.ActiveScene().BuildIndex + 1;
            SceneBinding nextScene = SceneHelper.FindSceneByIndex(buildIdx);
            Services.State.LoadScene(nextScene, null, SceneLoadFlags.NoLoadingScreen);
        }
    }
}