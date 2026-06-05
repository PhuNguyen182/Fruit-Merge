using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using _FruitMerge.Scripts.Input;
using ServiceLocators.Core;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class FruitDragController : MonoBehaviour
    {
        private const string LogTag = "FruitDragController";
        
        [SerializeField] private Transform leftEdge;
        [SerializeField] private Transform rightEdge;
        [SerializeField] private Transform center;
        [SerializeField] private Transform dragTarget;
        [SerializeField] private FruitSpawner fruitSpawner;

        private bool _isPointerUp;
        private bool _isPointerDown;
        private InputController _inputController;
        private FruitItem _currentDraggingFruitItem;

        private void Awake()
        {
            this._inputController = ServiceLocator.Global.Get<InputController>();
            this.fruitSpawner.InitializeFruitSpawner();
        }

        private void Update()
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
            float safeOffset = this._currentDraggingFruitItem.GetDistanceBetweenCenterToColliderEdge();
            movingHorizontal = Mathf.Clamp(movingHorizontal, 
                this.leftEdge.position.x + safeOffset, 
                this.rightEdge.position.x - safeOffset);

            Vector3 fruitDragPosition = new Vector3(movingHorizontal, movingVertical);
            this._currentDraggingFruitItem.transform.position = fruitDragPosition;
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
            this.fruitSpawner.SpawnNewFruit(this.dragTarget.position);
        }
    }
}
