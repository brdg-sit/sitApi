using EC_API.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EC_API.Controllers
{
    public class LoadHeatConsumption
    {
        /// <summary>
        /// model output class for EC_MLModel_1.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            [ColumnName(@"Score")]
            public float Score { get; set; }
        }
        #endregion

        public static string LoadHeatModelPath = "";

        public static readonly Lazy<PredictionEngine<LoadHeat, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<LoadHeat, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(LoadHeat input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<LoadHeat, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            DataViewSchema dataViewSchema;

            ITransformer mlModel = mlContext.Model.Load(LoadHeatModelPath, out dataViewSchema);
            Console.WriteLine(dataViewSchema);
            return mlContext.Model.CreatePredictionEngine<LoadHeat, ModelOutput>(mlModel, dataViewSchema);
        }
    }
}
