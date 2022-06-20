using EC_API.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System.Data;
using UnrealViewerAPI.Controllers;

namespace EC_API.Controllers
{
    public class EnergyTrainingController
    {
        private Transaction transaction = new Transaction();

        public string Train(string area, string eqmt, string type)
        {
            // Create MLContext
            MLContext mlContext = new MLContext();

            EstimatorChain<RegressionPredictionTransformer<PoissonRegressionModelParameters>> pipelineEstimator =
               mlContext.Transforms.Concatenate("Features", new string[] { "aspect_ratio", "temp_cool", "pwr_eqmt", "temp_heat", "level_light", "north_axis", "occupancy", "shgc", "u_floor", "u_roof", "u_wall", "u_window", "hur_wday", "hur_wend", "wwr", "effcy_cool", "effcy_heat" })
                   .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                   .Append(mlContext.Regression.Trainers.LbfgsPoissonRegression());

            string query = "";
            string table = "";

            List<LoadCool> loadCoolDatas = new List<LoadCool>();
            List<LoadHeat> loadHeatDatas = new List<LoadHeat>();
            List<LoadBaseElec> loadBaseElecDatas = new List<LoadBaseElec>();

            string dataSource = @"Server=sit01.brdg.kr,1433;Database=sit01;User Id=sit01;Password=sit123!@#;";

            if (eqmt == "ehp")
            {
                table = "tbl_ml_ehp_sample";
                query = String.Format("SELECT * FROM {0} WHERE area = {1};", table, area);
            }
            else if (eqmt == "central")
            {
                table = "tbl_ml_central_sample";
                query = String.Format("SELECT * FROM {0} WHERE area = {1};", table, area);
            }
            else
            {
                Console.WriteLine("bad option");
            }

            var dataTable = transaction.GetTableFromDB(query, dataSource);

            IDataView trainingData = null;

            if (type == "cool")
            {
                loadCoolDatas = (from DataRow dr in dataTable.Rows
                                 select new LoadCool()
                                 {
                                     load_cool = (float)Convert.ToDouble(dr["load_cool"]),
                                     //load_heat = (float)Convert.ToDouble(dr["load_heat"]),
                                     //load_baseElec = (float)Convert.ToDouble(dr["load_baseElec"]),
                                     aspect_ratio = (float)Convert.ToDouble(dr["aspect_ratio"]),
                                     temp_cool = (float)Convert.ToDouble(dr["temp_cool"]),
                                     pwr_eqmt = (float)Convert.ToDouble(dr["pwr_eqmt"]),
                                     temp_heat = (float)Convert.ToDouble(dr["temp_heat"]),
                                     level_light = (float)Convert.ToDouble(dr["level_light"]),
                                     north_axis = (float)Convert.ToDouble(dr["north_axis"]),
                                     occupancy = (float)Convert.ToDouble(dr["occupancy"]),
                                     shgc = (float)Convert.ToDouble(dr["shgc"]),
                                     u_floor = (float)Convert.ToDouble(dr["u_floor"]),
                                     u_roof = (float)Convert.ToDouble(dr["u_roof"]),
                                     u_wall = (float)Convert.ToDouble(dr["u_wall"]),
                                     u_window = (float)Convert.ToDouble(dr["u_window"]),
                                     hur_wday = (float)Convert.ToDouble(dr["hur_wday"]),
                                     hur_wend = (float)Convert.ToDouble(dr["hur_wend"]),
                                     wwr = (float)Convert.ToDouble(dr["wwr"]),
                                     effcy_cool = (float)Convert.ToDouble(dr["effcy_cool"]),
                                     effcy_heat = (float)Convert.ToDouble(dr["effcy_heat"])
                                 }).ToList();
                trainingData = mlContext.Data.LoadFromEnumerable(loadCoolDatas);

            }
            else if (type == "heat")
            {
                loadHeatDatas = (from DataRow dr in dataTable.Rows
                                 select new LoadHeat()
                                 {
                                     load_heat = (float)Convert.ToDouble(dr["load_heat"]),
                                     aspect_ratio = (float)Convert.ToDouble(dr["aspect_ratio"]),
                                     temp_cool = (float)Convert.ToDouble(dr["temp_cool"]),
                                     pwr_eqmt = (float)Convert.ToDouble(dr["pwr_eqmt"]),
                                     temp_heat = (float)Convert.ToDouble(dr["temp_heat"]),
                                     level_light = (float)Convert.ToDouble(dr["level_light"]),
                                     north_axis = (float)Convert.ToDouble(dr["north_axis"]),
                                     occupancy = (float)Convert.ToDouble(dr["occupancy"]),
                                     shgc = (float)Convert.ToDouble(dr["shgc"]),
                                     u_floor = (float)Convert.ToDouble(dr["u_floor"]),
                                     u_roof = (float)Convert.ToDouble(dr["u_roof"]),
                                     u_wall = (float)Convert.ToDouble(dr["u_wall"]),
                                     u_window = (float)Convert.ToDouble(dr["u_window"]),
                                     hur_wday = (float)Convert.ToDouble(dr["hur_wday"]),
                                     hur_wend = (float)Convert.ToDouble(dr["hur_wend"]),
                                     wwr = (float)Convert.ToDouble(dr["wwr"]),
                                     effcy_cool = (float)Convert.ToDouble(dr["effcy_cool"]),
                                     effcy_heat = (float)Convert.ToDouble(dr["effcy_heat"])
                                 }).ToList();
                trainingData = mlContext.Data.LoadFromEnumerable(loadHeatDatas);
            }
            else if (type == "elec")
            {
                loadBaseElecDatas = (from DataRow dr in dataTable.Rows
                                     select new LoadBaseElec()
                                     {
                                         load_baseElec = (float)Convert.ToDouble(dr["load_baseElec"]),
                                         aspect_ratio = (float)Convert.ToDouble(dr["aspect_ratio"]),
                                         temp_cool = (float)Convert.ToDouble(dr["temp_cool"]),
                                         pwr_eqmt = (float)Convert.ToDouble(dr["pwr_eqmt"]),
                                         temp_heat = (float)Convert.ToDouble(dr["temp_heat"]),
                                         level_light = (float)Convert.ToDouble(dr["level_light"]),
                                         north_axis = (float)Convert.ToDouble(dr["north_axis"]),
                                         occupancy = (float)Convert.ToDouble(dr["occupancy"]),
                                         shgc = (float)Convert.ToDouble(dr["shgc"]),
                                         u_floor = (float)Convert.ToDouble(dr["u_floor"]),
                                         u_roof = (float)Convert.ToDouble(dr["u_roof"]),
                                         u_wall = (float)Convert.ToDouble(dr["u_wall"]),
                                         u_window = (float)Convert.ToDouble(dr["u_window"]),
                                         hur_wday = (float)Convert.ToDouble(dr["hur_wday"]),
                                         hur_wend = (float)Convert.ToDouble(dr["hur_wend"]),
                                         wwr = (float)Convert.ToDouble(dr["wwr"]),
                                         effcy_cool = (float)Convert.ToDouble(dr["effcy_cool"]),
                                         effcy_heat = (float)Convert.ToDouble(dr["effcy_heat"])
                                     }).ToList();
                trainingData = mlContext.Data.LoadFromEnumerable(loadBaseElecDatas);
            }
            else
            {
                Console.Write("bad option");
            }

            DataOperationsCatalog.TrainTestData dataSplit = mlContext.Data.TrainTestSplit(trainingData, testFraction: 0.2);
            IDataView trainData = dataSplit.TrainSet;
            IDataView testData = dataSplit.TestSet;

            ITransformer trainedModel = pipelineEstimator.Fit(trainData);

            var savingDir = Path.Combine(Directory.GetCurrentDirectory(), "ML");

            if (!Directory.Exists(savingDir))
            {
                Directory.CreateDirectory(savingDir);
            }
            string resultPath = Path.Combine(savingDir, String.Format("{0}_{1}_{2}.zip", eqmt, type, area));
            mlContext.Model.Save(trainedModel, trainData.Schema, resultPath);

            //// Measure trained model performance
            //// Apply data prep transformer to test data
            IDataView transformedTestData = trainedModel.Transform(testData);

            //// Extract model metrics and get RSquared
            RegressionMetrics trainedModelMetrics = mlContext.Regression.Evaluate(transformedTestData);
            string resultStatusPath = Path.Combine(savingDir, String.Format("{0}_{1}_{2}.txt", eqmt, type, area));
            WriteMetrics(trainedModelMetrics, resultStatusPath);

            return string.Format("Rsquared: {0}\nRootMeanSquareError: {1}\nMeanSquaredError: {2}\nMeanAbsoluteError: {3}",
                trainedModelMetrics.RSquared,
                trainedModelMetrics.RootMeanSquaredError,
                trainedModelMetrics.MeanSquaredError,
                trainedModelMetrics.MeanAbsoluteError
            );
        }
        private static void WriteMetrics(RegressionMetrics metrics, string fileName)
        {
            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                // Check if file already exists. If yes, delete it.     
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.WriteLine($"RSquared: {metrics.RSquared}");
                    sw.WriteLine($"RootMeanSquareError: {metrics.RootMeanSquaredError}");
                    sw.WriteLine($"MeanSquaredError: {metrics.MeanSquaredError}");
                    sw.WriteLine($"MeanAbsoluteError: {metrics.MeanAbsoluteError}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
