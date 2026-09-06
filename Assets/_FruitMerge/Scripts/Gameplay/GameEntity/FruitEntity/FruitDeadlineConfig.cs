using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameEntity.FruitEntity
{
    [CreateAssetMenu(fileName = "FruitDeadlineConfig", menuName = "Scriptable Objects/FruitMerge/FruitDeadlineConfig")]
    public class FruitDeadlineConfig : ScriptableObject
    {
        public float deadlineDuration = 5f;
    }
}
