using System.Threading;
using _FruitMerge.Scripts.Gameplay.Factory.FruitFactory;
using _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity.Messages;
using Cysharp.Threading.Tasks;
using DracoRuan.CoreSystems.PlayerLoopSystem.Core.Handlers;
using UnityEngine;
using Random = UnityEngine.Random;
using MessagePipe;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity
{
    public class FruitItem : MonoBehaviour, IUpdateHandler
    {
        private const string LogTag = "FruitItem";

        [SerializeField] private FruitDeadlineConfig deadlineConfig;
        [SerializeField] private SpriteRenderer fruitRenderer;
        [SerializeField] private LayerMask fruitLayerMask;
        [SerializeField] private LayerMask barrierLayerMask;
        
        [Header("Fruit Physics")]
        [SerializeField] private Rigidbody2D fruitBody;
        [SerializeField] private CircleCollider2D fruitCollider;
        [SerializeField] private FruitConfig defaultFruitConfig;
        [SerializeField] private float minCenterTolerance;
        [SerializeField] private float maxCenterTolerance;
        
        private IPublisher<FruitReleaseMessage> _fruitReleasePublisher;
        private IPublisher<AddFruitScoreMessage> _addFruitScorePublisher;
        private IPublisher<FruitSpawnMessage> _fruitSpawnPublisher;
        
        private FruitItemFactory _fruitItemFactory;
        private CancellationToken _cancellationToken;
        private Vector2 _originalCenterOfMass;

        private float _timerCounter;
        private bool _isTouchToBarrier;
        private bool _hasResetCenterOfMass;
        private int _maxFruitLevel;

        public int FruitID { get; private set; }
        private bool IsFirstCollider { get; set; }
        public int FruitScore { get; private set; }

        private void Awake()
        {
            this._cancellationToken = this.GetCancellationTokenOnDestroy();
            this._originalCenterOfMass = this.fruitBody.centerOfMass;
            this.SetRandomMassCenter();
            
            this._fruitReleasePublisher = GlobalMessagePipe.GetPublisher<FruitReleaseMessage>();
            this._addFruitScorePublisher = GlobalMessagePipe.GetPublisher<AddFruitScoreMessage>();
            this._fruitSpawnPublisher = GlobalMessagePipe.GetPublisher<FruitSpawnMessage>();
        }

        private void SetRandomMassCenter()
        {
            Vector2 circleUnit = Random.insideUnitCircle.normalized;
            float centerTolerance = Random.Range(this.minCenterTolerance, this.maxCenterTolerance);
            this.fruitBody.centerOfMass = circleUnit * centerTolerance;
        }
        
        public void Tick(float deltaTime)
        {
            if (this._isTouchToBarrier && this._timerCounter < this.deadlineConfig.deadlineDuration)
            {
                this._timerCounter += deltaTime;
                if (this._timerCounter >= this.deadlineConfig.deadlineDuration)
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
                
            FruitItem upgradedFruit = this._fruitItemFactory.Create(fruitItemParam);
            this.AddScore(upgradedFruit);
            
            await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, this._cancellationToken);
            this.ReleaseFruit(this);
            this.ReleaseFruit(otherFruitItem);
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

        private void ReleaseFruit(FruitItem fruitItem)
        {
            this._fruitReleasePublisher.Publish(new FruitReleaseMessage
            {
                FruitInstanceID = this.gameObject.GetInstanceID(),
            });
            
            GameObjectPoolManager.Despawn(fruitItem.gameObject);
        }

        private void AddScore(FruitItem fruitItem)
        {
            this._addFruitScorePublisher.Publish(new AddFruitScoreMessage
            {
                FruitScore = fruitItem.FruitScore,
            });
            
            this.AddSpawnedFruitToMemory(fruitItem);
        }

        private void AddSpawnedFruitToMemory(FruitItem fruitItem)
        {
            this._fruitSpawnPublisher.Publish(new FruitSpawnMessage
            {
                FruitItem = fruitItem,
            });
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
            this.AddSpawnedFruitToMemory(this);
        }

        private void OnDisable()
        {
            this.IsFirstCollider = false;
            this.ApplyFruitConfig(this.defaultFruitConfig);
        }
    }
}
