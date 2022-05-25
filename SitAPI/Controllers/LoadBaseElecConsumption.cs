using EC_API.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace EC_API.Controllers
{
    public class LoadBaseElecConsumption
    {
        /// <summary>
        /// model output class for LoadBaseElec.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            [ColumnName(@"Score")]
            public float Score { get; set; }
        }
        #endregion

        public static string LoadBaseElecModelPath = "";

        public static readonly Lazy<PredictionEngine<LoadBaseElec, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<LoadBaseElec, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(LoadBaseElec input)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<LoadBaseElec, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            DataViewSchema dataViewSchema;

            ITransformer mlModel = mlContext.Model.Load(LoadBaseElecModelPath, out dataViewSchema);
            Console.WriteLine(dataViewSchema);
            return mlContext.Model.CreatePredictionEngine<LoadBaseElec, ModelOutput>(mlModel, dataViewSchema);
        }
    }
}
