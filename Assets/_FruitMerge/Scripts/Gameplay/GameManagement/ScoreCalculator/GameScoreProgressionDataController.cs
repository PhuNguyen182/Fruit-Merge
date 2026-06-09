using DracoRuan.Foundation.DataFlow.DataProviders;
using DracoRuan.Foundation.DataFlow.LocalData.DynamicDataControllers;
using DracoRuan.Foundation.DataFlow.SaveSystem;
using DracoRuan.Foundation.DataFlow.Serialization;

namespace _FruitMerge.Scripts.Gameplay.GameManagement.ScoreCalculator
{
    public class GameScoreProgressionDataController : DynamicGameDataController<ScoreProgressionData>
    {
        protected override ScoreProgressionData SourceData { get; set; }
        protected override IDataSerializer<ScoreProgressionData> DataSerializer { get; set; }
        protected override IDataSaveService DataSaveService { get; set; }
        protected override SerializationType SerializationType => SerializationType.Json;
        protected override DataSourceType DataSourceType => DataSourceType.File;
        
        public override void Initialize()
        {
        }

        public int GetCurrentHighScore() => this.SourceData.highScore;

        public void TrySaveHighestScore(int highestScore)
        {
            if (highestScore <= this.SourceData.highScore)
                return;
            
            this.SourceData.highScore = highestScore;
            this.SaveData();
        }
    }
}
