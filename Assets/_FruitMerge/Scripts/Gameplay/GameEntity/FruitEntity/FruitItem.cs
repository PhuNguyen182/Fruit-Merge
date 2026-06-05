using System.Threading;
using _FruitMerge.Scripts.Gameplay.Factory.FruitFactory;
using Cysharp.Threading.Tasks;
using DracoRuan.CoreSystems.PlayerLoopSystem.Core.Handlers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity
{
    public class FruitItem : MonoBehaviour, IUpdateHandler
    {
        private const string LogTag = "FruitItem";

        [SerializeField] private float deadlineDuration = 5f;
        [SerializeField] private SpriteRenderer fruitRenderer;
        [SerializeField] private LayerMask fruitLayerMask;
        [SerializeField] private LayerMask barrierLayerMask;
        
        [Header("Fruit Physics")]
        [SerializeField] private Rigidbody2D fruitBody;
        [SerializeField] private CircleCollider2D fruitCollider;
        [SerializeField] private FruitConfig defaultFruitConfig;
        [SerializeField] private float minCenterTolerance;
        [SerializeField] private float maxCenterTolerance;

        private FruitItemFactory _fruitItemFactory;
        private CancellationToken _cancellationToken;
        private Vector2 _originalCenterOfMass;

        private float _timerCounter;
        private bool _isTouchToBarrier;
        private bool _hasResetCenterOfMass;
        private int _maxFruitLevel;

        private int FruitID { get; set; }
        private bool IsFirstCollider { get; set; }
        public int FruitScore { get; private set; }

        private void Awake()
        {
            this._cancellationToken = this.GetCancellationTokenOnDestroy();
            this._originalCenterOfMass = this.fruitBody.centerOfMass;
            this.SetRandomMassCenter();
        }

        private void SetRandomMassCenter()
        {
            Vector2 circleUnit = Random.insideUnitCircle.normalized;
            float centerTolerance = Random.Range(this.minCenterTolerance, this.maxCenterTolerance);
            this.fruitBody.centerOfMass = circleUnit * centerTolerance;
        }
        
        public void Tick(float deltaTime)
        {
            if (this._isTouchToBarrier && this._timerCounter < this.deadlineDuration)
            {
                this._timerCounter += deltaTime;
                if (this._timerCounter >= this.deadlineDuration)
                {
                    // Lose game
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            this.TryResetCenterOfMass();
            this.OnColliderToOtherFruit(other);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & this.fruitLayerMask.value) != 0)
            {
                this._isTouchToBarrier = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & this.fruitLayerMask.value) == 0) 
                return;
            
            this._isTouchToBarrier = false;
            this._timerCounter = 0;
        }

        private void TryResetCenterOfMass()
        {
            if (this._hasResetCenterOfMass)
                return;

            Debug.Log($"[{LogTag}] Center of mass starting reset!");
            this._hasResetCenterOfMass = true;
            this.fruitBody.centerOfMass = this._originalCenterOfMass;
        }

        #region Check Fruit Execution

        private void OnColliderToOtherFruit(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & this.fruitLayerMask.value) == 0
                || !collision.collider.TryGetComponent(out FruitItem otherFruitItem)) 
                return;
            
            if (this.IsMaxFruitLevel())
            {
                Debug.Log($"[{LogTag}] Reach the max fruit level!");
                return;
            }
                
            this.TryMergeFruit(otherFruitItem);
        }

        private void TryMergeFruit(FruitItem otherFruitItem)
        {
            if (!this.IsSameFruit(otherFruitItem))
            {
                Debug.Log($"[{LogTag}] Not the same fruits!");
                return;
            }
            
            this.MergeFruitAsync(otherFruitItem).Forget();
        }
        
        private async UniTask MergeFruitAsync(FruitItem otherFruitItem)
        {
            if (!otherFruitItem.IsFirstCollider)
                this.IsFirstCollider = true;
                
            if (!this.IsFirstCollider)
                return;
                
            this.SetFruitColliderActive(false);
            otherFruitItem.SetFruitColliderActive(false);
                
            int nextFruitID = this.FruitID + 1;
            Vector2 averagePosition = (this.transform.position + otherFruitItem.transform.position) / 2f;
            FruitItemParam fruitItemParam = new FruitItemParam
            {
                FruitID = nextFruitID,
                Position = averagePosition,
            };
                
            this._fruitItemFactory.Create(fruitItemParam);
            await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, this._cancellationToken);
            GameObjectPoolManager.Despawn(this.gameObject);
            GameObjectPoolManager.Despawn(otherFruitItem.gameObject);
        }
        
        private bool IsSameFruit(FruitItem fruitItem)
        {
            bool isSameFruit = fruitItem.FruitID == this.FruitID;
            return isSameFruit;
        }

        private bool IsMaxFruitLevel()
        {
            bool isMaxFruitLevel = this.FruitID >= this._maxFruitLevel;
            return isMaxFruitLevel;
        }

        #endregion

        public void ApplyFruitConfig(FruitConfig fruitConfig)
        {
            this.FruitID = fruitConfig.fruitId;
            this.FruitScore = fruitConfig.fruitScore;
            this.fruitRenderer.sprite = fruitConfig.fruitIcon;
            this.fruitBody.mass = fruitConfig.fruitMass;
            this.fruitCollider.radius = fruitConfig.colliderRadius;
            this.fruitCollider.offset = fruitConfig.colliderOffset;
            this.fruitCollider.sharedMaterial = fruitConfig.fruitPhysicsMaterial;
        }
        
        public void SetMaxFruitLevel(int level) => this._maxFruitLevel = level;
        
        public void SetFruitPhysicsActive(bool isActive)
        {
            RigidbodyType2D bodyType = isActive ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
            this.fruitBody.bodyType = bodyType;
        }
        
        public void SetFruitColliderActive(bool isActive) => this.fruitCollider.enabled = isActive;

        public void Drop()
        {
            this.SetFruitColliderActive(true);
            this.SetFruitPhysicsActive(true);
        }

        private void OnDisable()
        {
            this.IsFirstCollider = false;
            this.ApplyFruitConfig(this.defaultFruitConfig);
        }
    }
}
