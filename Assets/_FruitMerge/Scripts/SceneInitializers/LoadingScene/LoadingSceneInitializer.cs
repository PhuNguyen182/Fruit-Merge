using Constants;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _FruitMerge.Scripts.SceneInitializers.LoadingScene
{
    public class LoadingSceneInitializer : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadSceneAsync(SceneName.MainHomeScene, LoadSceneMode.Single);
        }
    }
}
