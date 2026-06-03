using ZLinq;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Extensions
{
    public static class CollectionExtension
    {
        #region Array

        public static void Shuffle<T>(this T[] array, bool useSpecifySeed = false, int seed = 100)
        {
            int usingSeed = useSpecifySeed ? seed : (int)(DateTime.Now.Ticks % (int.MaxValue - 1));
            Random.InitState(usingSeed);

            for (int i = 0; i < array.Length - 1; i++)
            {
                int randomIndex = Random.Range(i + 1, array.Length);
                (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
            }
        }

        public static T GetLastElement<T>(this T[] array) => array[^1];

        public static T GetRandomElement<T>(this T[] array, bool useSpecifySeed = false, int seed = 100)
        {
            int usingSeed = useSpecifySeed ? seed : (int)(DateTime.Now.Ticks % (int.MaxValue - 1));
            Random.InitState(usingSeed);
            return array[Random.Range(0, array.Length)];
        }

        #endregion

        #region List

        public static void Shuffle<T>(this List<T> list, bool useSpecifySeed = false, int seed = 100)
        {
            int usingSeed = useSpecifySeed ? seed : (int)(DateTime.Now.Ticks % (int.MaxValue - 1));
            Random.InitState(usingSeed);

            for (int i = 0; i < list.Count - 1; i++)
            {
                int randomIndex = Random.Range(i + 1, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }

        public static T GetLastElement<T>(this List<T> list) => list[^1];

        public static T GetRandomElement<T>(this List<T> list, bool useSpecifySeed = false, int seed = 100)
        {
            int usingSeed = useSpecifySeed ? seed : (int)(DateTime.Now.Ticks % (int.MaxValue - 1));
            Random.InitState(usingSeed);
            return list[Random.Range(0, list.Count)];
        }

        #endregion

        #region HashSet

        public static T GetRandomElement<T>(this HashSet<T> hashSet, bool useSpecifySeed = false, int seed = 100)
        {
            int usingSeed = useSpecifySeed ? seed : (int)(DateTime.Now.Ticks % (int.MaxValue - 1));
            Random.InitState(usingSeed);
            int randomIndex = Random.Range(0, hashSet.Count);
            return hashSet.AsValueEnumerable().ElementAt(randomIndex);
        }

        public static bool AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> valueRange)
        {
            foreach (T value in valueRange)
            {
                bool canAdd = hashSet.Add(value);
                if (!canAdd)
                    return false;
            }
            
            return true;
        }

        #endregion

        #region Dictionary

        public static TValue GetRandomValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
            bool useSpecifySeed = false, int seed = 100)
        {
            int usingSeed = useSpecifySeed ? seed : (int)(DateTime.Now.Ticks % (int.MaxValue - 1));
            Random.InitState(usingSeed);
            int randomIndex = Random.Range(0, dictionary.Count);
            TKey randomKey = dictionary.Keys.AsValueEnumerable().ElementAt(randomIndex);
            return dictionary[randomKey];
        }

        #endregion
    }
}
