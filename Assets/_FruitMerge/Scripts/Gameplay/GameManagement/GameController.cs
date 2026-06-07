using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private FruitSpawner fruitSpawner;
        [SerializeField] private FruitDragController fruitDragController;
        
        private MessageBrokerManager _messageBrokerManager;
        
        private void Awake()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this._messageBrokerManager = new MessageBrokerManager();
            Debug.Log($"MessageBrokerManager initialized: {this._messageBrokerManager}");
            this.fruitSpawner.InitializeFruitSpawner();
            this.fruitDragController.InitializeFruitDragController();
        }
    }
}
