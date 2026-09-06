using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.UI.Misc
{
    [CreateAssetMenu(fileName = "FakeNameCollection", menuName = "Scriptable Objects/FruitMerge/FakeNameCollection")]
    public class FakeNameCollection : ScriptableObject
    {
        [SerializeField] public string[] fakeNames;

        public string GetRandomName()
        {
            int randomIndex = Random.Range(0, this.fakeNames.Length);
            return this.fakeNames[randomIndex];
        }
        
#if UNITY_EDITOR
        
        [SerializeField] public string rawNames;
        [SerializeField] public bool check;

        private void OnValidate()
        {
            if (this.check)
            {
                this.check = false;
                this.fakeNames = this.rawNames.Split(new[] { ',' });
            }
        }
        
#endif
    }
}
