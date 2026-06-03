using _FruitMerge.Scripts.Input;
using Cysharp.Threading.Tasks;
using DracoRuan.CoreSystems.AssetBundleSystem.Runtime;
using DracoRuan.Foundation.DataFlow.MasterDataController;
using ServiceLocators.Core;
using UnityEngine;

namespace ProjectScope
{
    public class ProjectInitializer : MonoBehaviour
    {
        [SerializeField] private InputController inputController;
        
        private IAssetBundleService _assetBundleService;
        private MainDataManager _mainDataManager;
        
        public bool AllServiceRegistered { get; private set; }

        private void Awake()
        {
            this.AllServiceRegistered = false;
            this.SetupProject();
            this.RegisterServices().Forget();
        }

        private void SetupProject()
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Application.targetFrameRate = Screen.currentResolution.refreshRateRatio.value <= 60
                ? 60
                : (int)Screen.currentResolution.refreshRateRatio.value;
#else
            Application.targetFrameRate = -1;
#endif
        }

        private async UniTask RegisterServices()
        {
            await this.RegisterAssetBundleService();
            await this.InitializeDataManager();
            this.RegisterInputController();
            this.AllServiceRegistered = true;
        }

        private async UniTask InitializeDataManager()
        {
            this._mainDataManager = new MainDataManager();
            await this._mainDataManager.InitializeDataHandlers();
            ServiceLocator.Global.Register(this._mainDataManager);
        }
        
        private async UniTask RegisterAssetBundleService()
        {
            this._assetBundleService = new AssetBundleService();
            await this._assetBundleService.Initialize();
            ServiceLocator.Global.Register(this._assetBundleService);
        }

        private void RegisterInputController()
        {
            ServiceLocator.Global.Register(this.inputController);
        }

        #region Data Saving
        
        private void OnApplicationQuit()
        {
            this._mainDataManager?.SaveAllData();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                this._mainDataManager?.SaveAllData();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                this._mainDataManager?.SaveAllData();
        }

        private void OnDestroy()
        {
            this._mainDataManager?.SaveAllData();
        }
        
        #endregion
    }
}
