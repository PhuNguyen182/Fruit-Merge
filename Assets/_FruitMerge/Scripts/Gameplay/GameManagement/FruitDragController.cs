using _FruitMerge.Scripts.Input;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using Cysharp.Threading.Tasks;
using DracoRuan.CoreSystems.PlayerLoopSystem.Core.Handlers;
using DracoRuan.CoreSystems.PlayerLoopSystem.UpdateServices;
using ServiceLocators.Core;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class FruitDragController : MonoBehaviour, IUpdateHandler
    {
        private const string LogTag = "FruitDragController";

        [SerializeField] private float fruitSpawnDelay = 1f;
        [SerializeField] private float fruitDragSpeed = 1.25f;
        [SerializeField] private float minFruitDragOffset = 0.1f;
        [SerializeField] private Transform leftEdge;
        [SerializeField] private Transform rightEdge;
        [SerializeField] private Transform center;
        [SerializeField] private Transform dragTarget;
        [SerializeField] private FruitSpawner fruitSpawner;

        private FruitItem _currentDraggingFruitItem;
        private InputController _inputController;

        private bool _canDragFruit;
        private bool _isPointerDown;
        private bool _isPointerUp;

        private void OnEnable()
        {
            UpdateServiceManager.RegisterUpdateHandler(this);
        }

        public void InitializeFruitDragController()
        {
            this._inputController = ServiceLocator.Global.Get<InputController>();
            this.dragTarget.position = this.center.position;
        }

        public void SetDragFruitEnabled(bool enable)
        {
            this._canDragFruit = enable;
        }

        public void SpawnStartFruit()
        {
            this._currentDraggingFruitItem = this.fruitSpawner.SpawnNewFruit(this.dragTarget.position);
        }

        public void Tick(float deltaTime)
        {
            if (!this._canDragFruit)
                return;
            
            bool isUIOverlapped = this._inputController.IsPointerOverlapUI();
            if (isUIOverlapped)
                return;
            
            this.TryDragFruit();
            this.TryDropFruit();
        }

        private void TryDragFruit()
        {
            this._isPointerDown = this._inputController.IsPointerClicked;
            if (!this._isPointerDown || !this._currentDraggingFruitItem)
                return;

            float pointerVelocity = this._inputController.WorldPointerVelocity.x;
            float movingVertical = this.dragTarget.position.y;
            float movingHorizontal = this.dragTarget.position.x + pointerVelocity * this.fruitDragSpeed;
            float safeOffset = this._currentDraggingFruitItem.GetSafeDistanceBetweenCenterToTankEdge();

            float minSafeDistance = this.leftEdge.position.x + safeOffset;
            float maxSafeDistance = this.rightEdge.position.x - safeOffset;
            movingHorizontal = Mathf.Clamp(movingHorizontal, minSafeDistance, maxSafeDistance);
            Vector3 fruitDragPosition = new Vector3(movingHorizontal, movingVertical);
            this.dragTarget.position = fruitDragPosition;
            this._currentDraggingFruitItem.transform.position = this.dragTarget.position;
            this._currentDraggingFruitItem.SetDropRayEnable(true);
        }

        private void TryDropFruit()
        {
            this._isPointerUp = this._inputController.IsPointerUp;
            if (!this._isPointerUp)
                return;
            
            if (!this._currentDraggingFruitItem)
            {
                Debug.Log($"[{LogTag}] No valid fruit to drag here!");
                return;
            }
            
            this._currentDraggingFruitItem.Drop();
            this._currentDraggingFruitItem = null;
            this.dragTarget.position = this.center.position;
            this.SpawnNewFruitWithDelay(this.fruitSpawnDelay).Forget();
        }

        private async UniTask SpawnNewFruitWithDelay(float delay)
        {
            await UniTask.WaitForSeconds(delay);
            this._currentDraggingFruitItem = this.fruitSpawner.SpawnNewFruit(this.dragTarget.position);
        }

        private void OnDisable()
        {
            UpdateServiceManager.DeregisterUpdateHandler(this);
        }
    }
}
