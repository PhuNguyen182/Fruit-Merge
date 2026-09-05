using _FruitMerge.Scripts.Gameplay.GameManagement.ScoreCalculator;
using _FruitMerge.Scripts.Gameplay.GameManagement.StateMachine;
using _FruitMerge.Scripts.Gameplay.GameTask;
using _FruitMerge.Scripts.Gameplay.GameTask.BoosterTasks;
using _FruitMerge.Scripts.Gameplay.UI.GameUI;
using _FruitMerge.Scripts.Input;
using _FruitMerge.Scripts.SceneInitializers.GameplayScene;
using DracoRuan.Foundation.DataFlow.MasterDataController;
using ServiceLocators.Core;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class FruitMergeGameController : MonoBehaviour
    {
        [SerializeField] private LayerMask fruitLayerMask;
        [SerializeField] private GameObject hammerPrefab;
        [SerializeField] private FruitSpawner fruitSpawner;
        [SerializeField] private FruitDragController fruitDragController;
        [SerializeField] private FruitDeadline fruitDeadline;

        private FruitMergeGameUI _gameUI;
        private InputController _inputController;
        private CameraVibrateTask _cameraVibrateTask;
        private GameStateController _gameStateController;
        private MessageBrokerManager _messageBrokerManager;
        private ScoreCalculationService _scoreCalculationService;
        private BoosterControllerTask _boosterControllerTask;
        private IMainDataManager _mainDataManager;
        
        public FruitSpawner FruitSpawner => this.fruitSpawner;

        public void InitializeGame()
        {
            this.Initialize();
            this.StartGame();
        }
        
        #region Initialization

        private void Initialize()
        {
            ServiceLocator.ForSceneOf(this).Register(this.fruitSpawner);
            this._mainDataManager = ServiceLocator.Global.Get<MainDataManager>();
            this._inputController = ServiceLocator.Global.Get<InputController>();
            this._inputController.ForceUpdateCameraToCurrentScene();
            
            this.InitializeMessageBroker();
            this.InitializeGameStateMachine();
            this.InitializeFruitMergeGame();
            this.InitializeGameScoreCalculator();
            this.InitializeCameraVibrateTask();
            this.InitializeGameUI();
        }

        private void InitializeGameUI()
        {
            this._gameUI = ServiceLocator.ForSceneOf(this).Get<FruitMergeGameUI>();
            this._gameUI.RegisterServices();
        }

        private void InitializeGameStateMachine()
        {
            this._gameStateController = new GameStateController(this.fruitDragController, this._inputController);
        }

        private void InitializeMessageBroker()
        {
            this._messageBrokerManager = new MessageBrokerManager();
            Debug.Log($"MessageBrokerManager initialized: {this._messageBrokerManager}");
        }

        private void InitializeFruitMergeGame()
        {
            this.fruitSpawner.InitializeFruitSpawner();
            this.fruitDragController.InitializeFruitDragController(this._inputController);
            this.InitializeFruitDeadline();
            this._boosterControllerTask = new BoosterControllerTask(this._inputController, this.hammerPrefab,
                this.fruitLayerMask, this.fruitSpawner.FruitMemory);
            ServiceLocator.ForSceneOf(this).Register(this._boosterControllerTask);
        }

        private void InitializeFruitDeadline()
        {
            this.fruitDeadline.InitializeFruitDeadline();
            this.fruitDeadline.OnFruitDeadlineTimeOut += this.EndGame;
        }

        private void InitializeGameScoreCalculator()
        {
            var scoreProgressionDataController =
                this._mainDataManager.GetDynamicDataController<GameScoreProgressionDataController>();
            this._scoreCalculationService = new ScoreCalculationService(scoreProgressionDataController);
            ServiceLocator.ForSceneOf(this).Register(this._scoreCalculationService);
        }

        private void InitializeCameraVibrateTask()
        {
            var gameplaySceneInitializer = ServiceLocator.ForSceneOf(this).Get<GameplaySceneInitializer>();
            var cameraShakeController = gameplaySceneInitializer.CameraShakeController;
            this._cameraVibrateTask = new CameraVibrateTask(cameraShakeController);
        }

        public void ContinueGame()
        {
            this._gameStateController.ContinueGame();
        }

        private void EndGame()
        {
            this._gameStateController.EndGame();
        }
        
        #endregion

        private void StartGame()
        {
            this._gameStateController.PlayGame();
            this.fruitDragController.SpawnStartFruit();
        }

        private void OnDestroy()
        {
            this.fruitDeadline.OnFruitDeadlineTimeOut -= this.EndGame;
            this._scoreCalculationService.Dispose();
            this._boosterControllerTask.Dispose();
            this._cameraVibrateTask.Dispose();
        }
    }
}
