using Cysharp.Threading.Tasks;
using ProjectScope;
using ServiceLocators.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneName = Constants.SceneName;

namespace _FruitMerge.Scripts.SceneInitializers.BootScene
{
    public class BootSceneInitializer : MonoBehaviour
    {
        private ProjectInitializer _projectInitializer;
        
        private void Start()
        {
            this._projectInitializer = ServiceLocator.ForSceneOf(this).Get<ProjectInitializer>();
            this.WaitForProjectInitialization().Forget();
        }

        private async UniTask WaitForProjectInitialization()
        {
            await UniTask.WaitUntil(IsProjectInitialized);
            await SceneManager.LoadSceneAsync(SceneName.LoadingScene, LoadSceneMode.Single);
            return;

            bool IsProjectInitialized() => this._projectInitializer.AllServiceRegistered;
        }
    }
}
