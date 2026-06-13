using Cysharp.Threading.Tasks;
using _FruitMerge.Scripts.Gameplay.GameEntity.Camera;
using _FruitMerge.Scripts.Gameplay.GameManagement;
using _FruitMerge.Scripts.Gameplay.GameTask;
using DracoRuan.CoreSystems.AssetBundleSystem.Runtime;
using DracoRuan.Foundation.UISystem.Canvases;
using DracoRuan.Foundation.UISystem.Popups.PopupManager;
using ServiceLocators.Core;
using UnityEngine;

namespace _FruitMerge.Scripts.SceneInitializers.GameplayScene
{
    public class GameplaySceneInitializer : MonoBehaviour
    {
        [SerializeField] private CameraShakeController cameraShakeController;
        [SerializeField] private PopupCollection popupCollection;
        [SerializeField] private UICanvasManager canvasManager;
        
        private CameraVibrateTask _cameraVibrateTask;
        private IAssetBundleService _assetBundleService;
        private IUIPopupManager _popupManager;
        
        public IUICanvasManager CanvasManager => this.canvasManager;
        public IUIPopupManager PopupManager => this._popupManager;

        private void Awake()
        {
            this.InitializeAssetBundleService();
            this.InitializeFruitMergeGame().Forget();
        }

        private void InitializeAssetBundleService()
        {
            ServiceLocator.ForSceneOf(this).Register(this);
            this._assetBundleService = ServiceLocator.Global.Get<IAssetBundleService>();
            this._popupManager = new UIPopupManager(this.popupCollection);
            this._cameraVibrateTask = new CameraVibrateTask(this.cameraShakeController);
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
