using _FruitMerge.Scripts.Gameplay.GameEntity.Camera;
using _FruitMerge.Scripts.Gameplay.GameManagement;
using DracoRuan.CoreSystems.AssetBundleSystem.Runtime;
using DracoRuan.Foundation.UISystem.Canvases;
using DracoRuan.Foundation.UISystem.Popups.PopupManager;
using ServiceLocators.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace _FruitMerge.Scripts.SceneInitializers.GameplayScene
{
    public class GameplaySceneInitializer : MonoBehaviour
    {
        private const float DefaultCameraSize = 10f;
        private const float StandardScreenRatio = 9f / 16f;
        
        [SerializeField] private Camera mainCamera;
        [SerializeField] private CinemachineCamera viewCamera;
        [SerializeField] private FruitMergeGameController fruitMergeGameController;
        [SerializeField] private CameraShakeController cameraShakeController;
        [SerializeField] private PopupCollection popupCollection;
        [SerializeField] private UICanvasManager canvasManager;
        
        private IAssetBundleService _assetBundleService;
        private IUIPopupManager _popupManager;
        
        public CameraShakeController CameraShakeController => this.cameraShakeController;
        public IUICanvasManager CanvasManager => this.canvasManager;
        public IUIPopupManager PopupManager => this._popupManager;

        private void Awake()
        {
            this.InitializeAssetBundleService();
            this.RecalculateCameraSizeByScreenRatio();
        }

        private void Start()
        {
            this.InitializeFruitMergeGame();
        }

        private void InitializeAssetBundleService()
        {
            ServiceLocator.ForSceneOf(this).Register(this);
            this._assetBundleService = ServiceLocator.Global.Get<IAssetBundleService>();
            this._popupManager = new UIPopupManager(this.popupCollection);
        }

        private void InitializeFruitMergeGame()
        {
            this.fruitMergeGameController.InitializeGame();
        }

        private void RecalculateCameraSizeByScreenRatio()
        {
            float currentCameraRatio = this.mainCamera.aspect;
            if (currentCameraRatio < StandardScreenRatio)
            {
                float scale = StandardScreenRatio / currentCameraRatio;
                this.viewCamera.Lens.OrthographicSize = scale * DefaultCameraSize;
            }
        }
    }
}
