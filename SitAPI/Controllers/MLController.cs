
using EC_API.Controllers;
using EC_API.Models;
using System.Data;
using UnrealViewerAPI.Controllers;

namespace SitAPI.Controllers
{
    public class MLController
    {
        private Transaction transaction = new Transaction();
        public float PredictLoadCool(string query, string dataSource)
        {
            //string query = string.Format("SELECT * FROM tbl_ml WHERE id_etr={0}", id_etr);
            //string dataSource = _configuration.GetConnectionString("PROD");

            var dataTable = transaction.GetTableFromDB(query, dataSource);

            if (dataTable.Rows.Count == 0)
            {
                return -1;
            }

            var row = dataTable.Rows[0];

            var is_ehp = (row["eqmt"].ToString() == "401") ? true : false;

            var area = row["area"].ToString();

            var data = (from DataRow dr in dataTable.Rows
                        select new LoadCool()
                        {
                            load_cool = 0,
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


            if (is_ehp)
            {
                var path = String.Format("ML/ehp_{0}_{1}.zip", "cool", area);
                LoadCoolConsumption.LoadCoolModelPath = Path.GetFullPath(path);
            }
            else
            {
                var path = String.Format("ML/central_{0}_{1}.zip", "cool", area);
                LoadCoolConsumption.LoadCoolModelPath = Path.GetFullPath(path);
            }


            var expectedLoadCool = LoadCoolConsumption.Predict(data.First()).Score;
            return expectedLoadCool;
        }

        public float PredictLoadHeat(string query, string dataSource)
        {
            //string query = string.Format("SELECT * FROM tbl_ml WHERE id_etr={0}", etr.id_etr);
            //string dataSource = _configuration.GetConnectionString("PROD");

            var dataTable = transaction.GetTableFromDB(query, dataSource);

            if (dataTable.Rows.Count == 0)
            {
                return -1;
            }

            var row = dataTable.Rows[0];

            var is_ehp = (row["eqmt"].ToString() == "401") ? true : false;

            var area = row["area"].ToString();

            var data = (from DataRow dr in dataTable.Rows
                        select new LoadHeat()
                        {
                            load_heat = 0,
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


            if (is_ehp)
            {
                var path = String.Format("ML/ehp_{0}_{1}.zip", "heat", area);
                LoadHeatConsumption.LoadHeatModelPath = Path.GetFullPath(path);
            }
            else
            {
                var path = String.Format("ML/central_{0}_{1}.zip", "heat", area);
                LoadHeatConsumption.LoadHeatModelPath = Path.GetFullPath(path);
            }

            var expectedLoadHeat = LoadHeatConsumption.Predict(data.First()).Score;
            return expectedLoadHeat;
        }

        public float PredictLoadBaseElec(string query, string dataSource)
        {
            //string query = string.Format("SELECT * FROM tbl_ml WHERE id_etr={0}", etr.id_etr);
            //string dataSource = _configuration.GetConnectionString("PROD");

            var dataTable = transaction.GetTableFromDB(query, dataSource);

            if (dataTable.Rows.Count == 0)
            {
                return -1;
            }

            var row = dataTable.Rows[0];

            var is_ehp = (row["eqmt"].ToString() == "401") ? true : false;

            var area = row["area"].ToString();

            var data = (from DataRow dr in dataTable.Rows
                        select new LoadBaseElec()
                        {
                            load_baseElec = 0,
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

            if (is_ehp)
            {
                var path = String.Format("ML/ehp_{0}_{1}.zip", "elec", area);
                LoadBaseElecConsumption.LoadBaseElecModelPath = Path.GetFullPath(path);
            }
            else
            {
                var path = String.Format("ML/central_{0}_{1}.zip", "elec", area);
                LoadBaseElecConsumption.LoadBaseElecModelPath = Path.GetFullPath(path);
            }

            var expectedLoadBaseElec = LoadBaseElecConsumption.Predict(data.First()).Score;
            return expectedLoadBaseElec;
        }
    }
}
