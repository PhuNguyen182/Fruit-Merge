using _FruitMerge.Scripts.Input;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity;
using Cysharp.Threading.Tasks;
using DracoRuan.CoreSystems.PlayerLoopSystem.Core.Handlers;
using DracoRuan.CoreSystems.PlayerLoopSystem.UpdateServices;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameManagement
{
    public class FruitDragController : MonoBehaviour, IUpdateHandler
    {
        private const string LogTag = "FruitDragController";
        
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] dropClips;
        [SerializeField] private float fruitSpawnDelay = 1f;
        [SerializeField] private float fruitDragSpeed = 1.25f;
        [SerializeField] private float minFruitDragOffset = 0.1f;
        [SerializeField] private Transform leftEdge;
        [SerializeField] private Transform rightEdge;
        [SerializeField] private Transform center;
        [SerializeField] private Transform dragTarget;
        [SerializeField] private FruitSpawner fruitSpawner;
        [SerializeField] private bool resetDragTargetToCenter;

        private FruitItem _currentDraggingFruitItem;
        private InputController _inputController;

        private bool _isUIOverlapped;
        private bool _canDragFruit;
        private bool _isPointerDown;
        private bool _isPointerUp;

        private void OnEnable()
        {
            UpdateServiceManager.RegisterUpdateHandler(this);
        }

        public void InitializeFruitDragController(InputController inputController)
        {
            this._inputController = inputController;
            this.dragTarget.position = this.center.position;
        }

        public void SetDragFruitEnabled(bool enable)
        {
            this._canDragFruit = enable;
        }

        public void SpawnStartFruit()
        {
            this._currentDraggingFruitItem = this.fruitSpawner.SpawnNewFruit(this.dragTarget.position);
            this._currentDraggingFruitItem.PlayStartAnimation();
            this._currentDraggingFruitItem.SetFadeRayEnable(true);
        }

        public void Tick(float deltaTime)
        {
            this.TryDisableFruitRay();
            if (!this._canDragFruit || !this._inputController.IsInputActive)
                return;

            this._isUIOverlapped = this._inputController.IsPointerOverlapUI();
            if (this._isUIOverlapped)
                return;

            this.TryDragFruit();
            this.TryDropFruit();
        }

        private void TryDragFruit()
        {
            this._isPointerDown = this._inputController.IsPointerClicked;
            if (!this._isPointerDown || !this._currentDraggingFruitItem || this._isUIOverlapped)
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
            this._currentDraggingFruitItem.SetDropRayEnable(!this._isUIOverlapped && this._inputController.IsInputActive);
            this._currentDraggingFruitItem.SetFadeRayEnable(!this._isUIOverlapped && !this._inputController.IsInputActive);
        }

        private void TryDropFruit()
        {
            this._isPointerUp = this._inputController.IsPointerUp;
            if (!this._isPointerUp || this._isUIOverlapped)
                return;

            if (!this._currentDraggingFruitItem)
            {
                Debug.Log($"[{LogTag}] No valid fruit to drag here!");
                return;
            }

            this._currentDraggingFruitItem.Drop();
            this._currentDraggingFruitItem = null;
            
            if (this.resetDragTargetToCenter)
                this.dragTarget.position = this.center.position;
            
            this.PlayMergeSound();
            this.SpawnNewFruitWithDelay(this.fruitSpawnDelay).Forget();
        }

        private void TryDisableFruitRay()
        {
            if (this._currentDraggingFruitItem && !this._inputController.IsInputActive)
            {
                this._currentDraggingFruitItem.SetDropRayEnable(false);
                this._currentDraggingFruitItem.SetFadeRayEnable(false);
            }
        }

        private async UniTask SpawnNewFruitWithDelay(float delay)
        {
            await UniTask.WaitForSeconds(delay);
            this._currentDraggingFruitItem = this.fruitSpawner.SpawnNewFruit(this.dragTarget.position);
            this._currentDraggingFruitItem.PlayStartAnimation();
            this._currentDraggingFruitItem.SetFadeRayEnable(true);
        }
        
        private void PlayMergeSound()
        {
            int rand = Random.Range(0, this.dropClips.Length);
            AudioClip mergeClip = this.dropClips[rand];
            this.audioSource.PlayOneShot(mergeClip);
        }

        private void OnDisable()
        {
            UpdateServiceManager.DeregisterUpdateHandler(this);
        }
    }
}
