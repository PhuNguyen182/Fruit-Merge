using _FruitMerge.Scripts.Input;
using DracoRuan.Foundation.UISystem.Canvases;
using DracoRuan.Foundation.UISystem.Popups.PopupManager;
using ServiceLocators.Core;
using UnityEngine;

namespace _FruitMerge.Scripts.SceneInitializers.HomeScene
{
    public class HomeSceneInitializer : MonoBehaviour
    {
        [SerializeField] private UICanvasManager canvasManager;
        [SerializeField] private PopupCollection popupCollection;
        
        private InputController _inputController;
        private IUIPopupManager _popupManager;
        
        public IUICanvasManager CanvasManager => this.canvasManager;
        public IUIPopupManager PopupManager => this._popupManager;

        private void Awake()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            ServiceLocator.ForSceneOf(this).Register(this);
            this._inputController = ServiceLocator.Global.Get<InputController>();
            this._popupManager = new UIPopupManager(this.popupCollection);
            this._inputController.ForceUpdateCameraToCurrentScene();
        }
    }
}
