using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity
{
    public class FruitItem : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D fruitBody;
        [SerializeField] private CircleCollider2D fruitCollider;
        [SerializeField] private SpriteRenderer fruitRenderer;
        
        public int FruitID { get; private set; }
        public int FruitScore { get; private set; }

        public void ApplyFruitConfig(FruitConfig fruitConfig)
        {
            this.FruitID = fruitConfig.fruitId;
            this.FruitScore = fruitConfig.fruitScore;
            this.fruitRenderer.sprite = fruitConfig.fruitIcon;
            this.fruitBody.mass = fruitConfig.fruitMass;
            this.fruitCollider.radius = fruitConfig.colliderRadius;
            this.fruitCollider.sharedMaterial = fruitConfig.fruitPhysicsMaterial;
        }
    }
}
