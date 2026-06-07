using _FruitMerge.Scripts.Input;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using DracoRuan.CoreSystems.PlayerLoopSystem.Core.Handlers;
using DracoRuan.CoreSystems.PlayerLoopSystem.UpdateServices;
using ServiceLocators.Core;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class FruitDragController : MonoBehaviour, IUpdateHandler
    {
        private const string LogTag = "FruitDragController";
        
        [SerializeField] private Transform leftEdge;
        [SerializeField] private Transform rightEdge;
        [SerializeField] private Transform center;
        [SerializeField] private Transform dragTarget;
        [SerializeField] private FruitSpawner fruitSpawner;

        private FruitItem _currentDraggingFruitItem;
        private InputController _inputController;
        
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

        public void SpawnStartFruit()
        {
            this.fruitSpawner.SpawnNewFruit(this.dragTarget.position);
        }

        public void Tick(float deltaTime)
        {
            bool isUIOverlapped = this._inputController.IsPointerOverlapUI();
            if (isUIOverlapped)
                return;
            
            this.TryDragFruit();
            this.TryDropFruit();
        }

        private void TryDragFruit()
        {
            this._isPointerDown = this._inputController.IsPointerDown;
            if (!this._isPointerDown || !this._currentDraggingFruitItem)
                return;
            
            Vector3 pointerDelta = this._inputController.PointerDelta;
            float movingVertical = this.dragTarget.position.y;
            float movingHorizontal = this.dragTarget.position.x + pointerDelta.x;
            float safeOffset = this._currentDraggingFruitItem.GetSafeDistanceBetweenCenterToTankEdge();
            
            float minSafeDistance = this.leftEdge.position.x + safeOffset;
            float maxSafeDistance = this.rightEdge.position.x - safeOffset;
            movingHorizontal = Mathf.Clamp(movingHorizontal,minSafeDistance,maxSafeDistance); 
            Vector3 fruitDragPosition = new Vector3(movingHorizontal, movingVertical);
            this._currentDraggingFruitItem.transform.position = fruitDragPosition;
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
            this._currentDraggingFruitItem = this.fruitSpawner.SpawnNewFruit(this.dragTarget.position);
        }

        private void OnDisable()
        {
            UpdateServiceManager.DeregisterUpdateHandler(this);
        }
    }
}
