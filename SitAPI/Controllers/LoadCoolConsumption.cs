using EC_API.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EC_API.Controllers
{
    public partial class LoadCoolConsumption
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

        public static string LoadCoolModelPath = "";

        public static readonly Lazy<PredictionEngine<LoadCool, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<LoadCool, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(LoadCool input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<LoadCool, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            DataViewSchema dataViewSchema;
            
            ITransformer mlModel = mlContext.Model.Load(LoadCoolModelPath, out dataViewSchema);
            Console.WriteLine(dataViewSchema);
            return mlContext.Model.CreatePredictionEngine<LoadCool, ModelOutput>(mlModel, dataViewSchema);
        }
    }
}
