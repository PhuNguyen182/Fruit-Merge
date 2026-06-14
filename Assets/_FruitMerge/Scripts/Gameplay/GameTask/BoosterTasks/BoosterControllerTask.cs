using System;
using _FruitMerge.Scripts.Gameplay.GameManagement;
using _FruitMerge.Scripts.Input;
using UnityEngine;

namespace _FruitMerge.Scripts.Gameplay.GameTask.BoosterTasks
{
    public class BoosterControllerTask : IDisposable
    {
        public HammerBoosterTask HammerBoosterTask { get; }

        public BoosterControllerTask(InputController inputController, GameObject hammerBoosterPrefab,
            LayerMask fruitLayerMask, FruitMemory fruitMemory)
        {
            this.HammerBoosterTask =
                new HammerBoosterTask(inputController, hammerBoosterPrefab, fruitLayerMask, fruitMemory);
        }

        public void Dispose()
        {
            this.HammerBoosterTask.Dispose();
        }
    }
}