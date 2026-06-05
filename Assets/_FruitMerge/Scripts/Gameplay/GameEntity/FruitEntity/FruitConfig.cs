using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity
{
    [CreateAssetMenu(fileName = "FruitConfig", menuName = "Scriptable Objects/FruitMerge/FruitConfig")]
    public class FruitConfig : ScriptableObject
    {
        [Header("Fruit Identity")]
        public int fruitId;
        public Sprite fruitIcon;
        public int fruitScore;
        public ParticleSystem fruitParticles;
        
        [Header("Fruit Config")]
        public float fruitMass = 1;
        public float colliderRadius = 0.5f;
        public Vector2 colliderOffset;
        public PhysicsMaterial2D fruitPhysicsMaterial;
    }
}
