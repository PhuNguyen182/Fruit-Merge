using System;
using DracoRuan.Foundation.DataFlow.LocalData;

namespace _FruitMerge.Scripts.Gameplay.GameManagement.ScoreCalculator
{
    [Serializable]
    [GameData(nameof(ScoreProgressionData))]
    public class ScoreProgressionData : IGameData, IDisposable
    {
        public int Version { get; set; }

        public int highScore;

        public void Dispose()
        {
            
        }
    }
}
