using _FruitMerge.Scripts.Gameplay.GameManagement.ScoreCalculator;
using _FruitMerge.Scripts.Gameplay.GameManagement.StateMachine;
using DracoRuan.Foundation.DataFlow.MasterDataController;
using ServiceLocators.Core;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class FruitMergeGameController : MonoBehaviour
    {
        [SerializeField] private FruitSpawner fruitSpawner;
        [SerializeField] private FruitDragController fruitDragController;
        [SerializeField] private FruitDeadline fruitDeadline;

        private GameStateController _gameStateController;
        private MessageBrokerManager _messageBrokerManager;
        private ScoreCalculationService _scoreCalculationService;
        private IMainDataManager _mainDataManager;

        public void InitializeGame()
        {
            this.Initialize();
            this.StartGame();
        }
        
        #region Initialization

        private void Initialize()
        {
            this._mainDataManager = ServiceLocator.Global.Get<MainDataManager>();
            this.InitializeMessageBroker();
            this.InitializeGameStateMachine();
            this.InitializeFruitMergeGame();
            this.InitializeGameScoreCalculator();
        }

        private void InitializeGameStateMachine()
        {
            this._gameStateController = new GameStateController(this.fruitDragController);
        }

        private void InitializeMessageBroker()
        {
            this._messageBrokerManager = new MessageBrokerManager();
            Debug.Log($"MessageBrokerManager initialized: {this._messageBrokerManager}");
        }

        private void InitializeFruitMergeGame()
        {
            this.fruitSpawner.InitializeFruitSpawner();
            this.fruitDragController.InitializeFruitDragController();
             this.InitializeFruitDeadline();
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
        }
    }
}
