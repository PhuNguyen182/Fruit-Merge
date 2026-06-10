using Cysharp.Threading.Tasks;
using _FruitMerge.Scripts.Gameplay.GameManagement;
using DracoRuan.CoreSystems.AssetBundleSystem.Runtime;
using ServiceLocators.Core;
using UnityEngine;

namespace _FruitMerge.Scripts.SceneInitializers.GameplayScene
{
    public class GameplaySceneInitializer : MonoBehaviour
    {
        private IAssetBundleService _assetBundleService;

        private void Awake()
        {
            this.InitializeAssetBundleService();
            this.InitializeFruitMergeGame().Forget();
        }

        private void InitializeAssetBundleService()
        {
            this._assetBundleService = ServiceLocator.Global.Get<IAssetBundleService>();
        }

        private async UniTaskVoid InitializeFruitMergeGame()
        {
            GameObject fruitMergeGamePrefab =
                await this._assetBundleService.AssetBundleLoader.LoadAsset("FruitMergeGame");
            if (fruitMergeGamePrefab)
            {
                GameObject fruitMergeGame = Instantiate(fruitMergeGamePrefab);
                if (fruitMergeGame &&
                    fruitMergeGame.TryGetComponent(out FruitMergeGameController fruitMergeGameController))
                {
                    fruitMergeGameController.InitializeGame();
                }
            }
        }
    }
}
