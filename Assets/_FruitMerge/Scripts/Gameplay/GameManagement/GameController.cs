using _FruitMerge.Scripts.Gameplay.GameManagement.StateMachine;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private FruitSpawner fruitSpawner;
        [SerializeField] private FruitDragController fruitDragController;

        private GameStateController _gameStateController;
        private MessageBrokerManager _messageBrokerManager;
        
        private void Awake()
        {
            this.Initialize();
        }

        private void Start()
        {
            this.StartGame();
        }

        #region Initialization

        private void Initialize()
        {
            this.InitializeMessageBroker();
            this.InitializeGameStateMachine();
            this.InitializeFruitMergeGame();
        }

        private void InitializeGameStateMachine()
        {
            this._gameStateController = new GameStateController();
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
        }
        
        #endregion

        private void StartGame()
        {
            this._gameStateController.PlayGame();
        }
    }
}
