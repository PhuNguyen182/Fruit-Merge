using _FruitMerge.Scripts.Input;

namespace _FruitMerge.Scripts.Gameplay.GameTask
{
    public class BoosterControllerTask
    {
        public HammerBoosterTask HammerBoosterTask { get; }

        public BoosterControllerTask(InputController inputController)
        {
            this.HammerBoosterTask = new HammerBoosterTask(inputController);
        }
    }
}