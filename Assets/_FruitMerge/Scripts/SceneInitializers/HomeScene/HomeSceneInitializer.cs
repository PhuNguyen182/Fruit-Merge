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
            this._popupManager = new UIPopupManager(this.popupCollection);
        }
    }
}
