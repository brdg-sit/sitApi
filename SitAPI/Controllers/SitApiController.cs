using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Web;
using System;
using Microsoft.Web.Administration;
using System.Net;
using System.Transactions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Data.SqlClient;
using System.Web.Helpers;
using EC_API.Controllers;
using EC_API.Models;
using SitAPI.Controllers;

namespace UnrealViewerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Sit : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private Transaction transaction = new Transaction();
        private MLController mlController = new MLController();

        public Sit(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        [HttpGet]
        [Route("mldata")]
        public string GetMlData()
        {
            string query = @"SELECT * FROM tbl_ml";

            string dataSource = _configuration.GetConnectionString("PROD");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("mldata")]
        public string GetMlData(string id_etr)
        {
            string query = $"SELECT * FROM tbl_ml WHERE id_etr = {id_etr}";

            string dataSource = _configuration.GetConnectionString("PROD");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("userdata")]
        public string GetUserData()
        {
            string query =
                $"SELECT " +
                    $"A.id, " +
                    $"A.address, " +
                    $"(select name from tbl_com_code where code = A.cd_north_axis) as cd_north_axis, " +
                    $"(select name from tbl_com_code where code = A.cd_usage_main) as cd_usage_main, " +
                    $"A.usage_sub, " +
                    $"A.year, " +
                    $"A.area, " +
                    $"A.wwr, " +
                    $"A.aspect_ratio, " +
                    $"A.u_wall, " +
                    $"A.u_roof, " +
                    $"A.u_floor, " +
                    $"A.u_window, " +
                    $"A.shgc, " +
                    $"(select name from tbl_com_code where code = A.cd_eqmt) as cd_eqmt, " +
                    $"(select name from tbl_com_code where code = A.cd_eqmt_light) as cd_eqmt_light, " +
                    $"A.effcy_heat, " +
                    $"A.effcy_cool, " +
                    $"A.level_light, " +
                    $"A.hur_wday, " +
                    $"A.hur_wend, " +
                    $"A.men_rsdt, " +
                    $"A.men_norsdt, " +
                    $"A.temp_heat, " +
                    $"A.temp_cool, " +
                    $"(select name from tbl_com_code where code = A.cd_unitgas) as cd_unitgas, " +
                    $"A.area_etr, " +
                    $"A.dt_create " +
                $"FROM " +
                    $"tbl_user_enter A";

            string dataSource = _configuration.GetConnectionString("PROD");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("tableinfo")]
        public string GetProjects(string nmTable)
        {
            string query = $"SELECT name as field, label as headerName, editable FROM tbl_def_cols WHERE [table] = '{nmTable}' order by ORDER_SORT";

            string dataSource = _configuration.GetConnectionString("PROD");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpPost]
        [Route("typload")]
        public string GetTypLoads(object obj)
        {
            JObject? jobj = JObject.Parse(obj.ToString());
            if (jobj.Count == 0)
            {
                return "";
            }

            var ID = jobj["id"];

            string query =
                $"SELECT " +
                    $"A.id, " +
                    $"A.id_etr, " +
                    $"A.mnth, " +
                    $"A.load_gas, " +
                    $"(select value from tbl_com_code where code = A.unit_gas) as unit_gas, " +
                    $"A.load_elec, " +
                    $"(select value from tbl_com_code where code = A.unit_elec) as unit_elec " +
                $"FROM " +
                    $"tbl_load_energy_typ A " +
                $"WHERE " +
                    $"A.id_etr = {ID}";

            string dataSource = _configuration.GetConnectionString("PROD");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpPost]
        [Route("usgload")]
        public string GetUsgLoads(object obj)
        {
            JObject? jobj = JObject.Parse(obj.ToString());
            if (jobj.Count == 0)
            {
                return "";
            }

            var ID = jobj["id"];

            string query =
                $"SELECT " +
                    $"A.id, " +
                    $"A.id_etr, " +
                    $"A.mnth, " +
                    $"A.load_cool, " +
                    $"(select value from tbl_com_code where code = A.unit_cool) as unit_cool, " +
                    $"A.load_heat, " +
                    $"(select value from tbl_com_code where code = A.unit_cool) as unit_heat, " +
                    $"A.load_baseElec, " +
                    $"(select value from tbl_com_code where code = A.unit_cool) as unit_baseElec, " +
                    $"A.load_baseGas, " +
                    $"(select value from tbl_com_code where code = A.unit_cool) as unit_baseGasc " +
                $"FROM " +
                    $"tbl_load_energy_usg A " +
                $"WHERE " +
                    $"A.id_etr = {ID} AND A.is_sep = 1";

            string dataSource = _configuration.GetConnectionString("PROD");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpPost]
        [Route("delete")]
        public string DeleteRows(object obj)
        {
            JObject? jobj = JObject.Parse(obj.ToString());
            if (jobj.Count == 0)
            {
                return "";
            }

            var ids = jobj["id"];
            if (ids.Count() == 0)
                return "";

            try
            {
                string joinIds = string.Empty;
                for (int i = 0; i < ids.Count(); i++)
                {
                    joinIds += $"{ids[i]},";
                }
                joinIds = joinIds.Substring(0, joinIds.Length - 1);

                string query =
                    $"DELETE FROM " +
                        $"tbl_load_energy_typ " +
                    $"WHERE " +
                        $"id_etr in ({joinIds})";

                string dataSource = _configuration.GetConnectionString("PROD");
                transaction.GetTableFromDB(query, dataSource);

                query =
                    $"DELETE FROM " +
                        $"tbl_load_energy_usg " +
                    $"WHERE " +
                        $"id_etr in ({joinIds})";

                transaction.GetTableFromDB(query, dataSource);

                query =
                    $"DELETE FROM " +
                        $"tbl_user_enter " +
                    $"WHERE " +
                        $"id in ({joinIds})";

                return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex);
            }
        }

        [HttpPost]
        [Route("save")]
        public string SaveRow(object obj)
        {
            JObject? jobj = JObject.Parse(obj.ToString());
            if (jobj.Count == 0)
            {
                return "";
            }

            var ID = jobj["id"];

            try
            {
                string query =
                    $"SELECT " +
                        $"count(*) " +
                    $"FROM " +
                        $"tbl_user_enter A " +
                    $"WHERE " +
                        $"A.id = {ID}";

                string dataSource = _configuration.GetConnectionString("PROD");

                var dt = transaction.GetTableFromDB(query, dataSource);
               var count =  Convert.ToInt32(dt.Rows[0][0]);

                if (count == 0)
                {
                    query = $"INSERT INTO tbl_user_enter(";
                    string fields = string.Empty;
                    int i = 0;
                    foreach (var j in jobj)
                    {
                        if (j.Key != "id")
                        {
                            fields += $"{j.Key},";
                            i++;
                        }
                    }
                    fields = fields.Substring(0, fields.Length - 1);
                    fields += ") ";

                    string values = "VALUES(";
                    i = 0;
                    foreach (var j in jobj)
                    {
                        if (j.Key != "id")
                        {
                            if (j.Key == "dt_create")
                            {
                                values += $"GETDATE(),";
                            }
                            else
                            {
                                values += $"'{j.Value}',";
                            }
                            i++;
                        }
                    }
                    values = values.Substring(0, values.Length - 1);
                    values += ");";

                    query += fields + values;
                }
                else
                {
                    // update
                    query = $"UPDATE tbl_user_enter SET ";
                    int i = 0;
                    foreach (var j in jobj)
                    {
                        if (j.Key != "id")
                        {
                            if (j.Key == "dt_create")
                            {
                            }
                            else
                            {
                                query += $"{j.Key} = '{j.Value}',";
                            }
                            i++;
                        }
                    }
                    query = query.Substring(0, query.Length - 1);

                    query += $" WHERE id = {ID}";
                }

                return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex);
            }
        }

        [HttpGet]
        [Route("defaults")]
        public string GetDefaults()
        {
            string query = @"SELECT * FROM tbl_user_enter_def";
            string dataSource = _configuration.GetConnectionString("PROD");
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("defaults2")]
        public string GetDefaults2()
        {
            string query = @"SELECT * FROM tbl_user_enter_def; SELECT TOP 1 * FROM tbl_user_enter; ";
            string dataSource = _configuration.GetConnectionString("PROD");
            return JsonConvert.SerializeObject(transaction.GetTablesFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("codes")]
        public string GetCodes()
        {
            string query = @"SELECT * FROM tbl_com_code";
            string dataSource = _configuration.GetConnectionString("PROD");
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("uval")]
        public string GetUVals()
        {
            string query = @"SELECT * FROM tbl_uval_year";
            string dataSource = _configuration.GetConnectionString("PROD");
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("get-energytyp")]
        public string GetEnergyType(string id_etr)
        {
            string dataSource = _configuration.GetConnectionString("PROD");
            string query = $"SELECT * FROM tbl_load_energy_typ WHERE id_etr=" + id_etr;
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("get-energyusage")]
        public string GetEnergyUsage(string id_etr)
        {
            string dataSource = _configuration.GetConnectionString("PROD");

            string query = $"SELECT area, cd_eqmt, hur_wday, hur_wend FROM tbl_user_enter WHERE id = {id_etr}";
            var dt = transaction.GetTableFromDB(query, dataSource);
            var area = dt.Rows[0]["area"];
            var cd_eqmt = dt.Rows[0]["cd_eqmt"];
            var hur_wday = dt.Rows[0]["hur_wday"];
            var hur_wend = dt.Rows[0]["hur_wend"];

            //string mlJson = JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
            //string mlStddJson = JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));

            // ML 예측 ===========
            // mlJson: 사용자입력값 예측 부분, cool, heat, baseElec 3번 실행
            // mlStddJson: 일반사용형태 예측 부분, cool, heat, baseElec 3번 실행
            // 총 6번 예측 실행
            query = $"SELECT * FROM tbl_ml_stdd WHERE id_etr={id_etr}";
            double mlStdd_load_cool = mlController.PredictLoadCool(query, dataSource);
            double mlStdd_load_heat = mlController.PredictLoadHeat(query, dataSource);
            double mlStdd_load_baseElec = mlController.PredictLoadBaseElec(query, dataSource);

            query = $"SELECT * FROM tbl_ml WHERE id_etr={id_etr}";
            double ml_load_cool = mlController.PredictLoadCool(query, dataSource);
            double ml_load_heat = mlController.PredictLoadHeat(query, dataSource);
            double ml_load_baseElec = mlController.PredictLoadBaseElec(query, dataSource);
            // ==================

            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = _configuration.GetConnectionString("PROD");
                connection.Open();

                string queryUpdate =
                    $"UPDATE " +
                        $"tbl_ml " +
                    $"SET " +
                        $"load_cool = {ml_load_cool}, " +
                        $"load_heat = {ml_load_heat}, " +
                        $"load_baseElec = {ml_load_baseElec} " +
                    $"WHERE " +
                        $"id_etr = {id_etr}";

                using (SqlCommand command = new SqlCommand(queryUpdate, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            // 용도별 에너지 비율
            double rate_load_cool = mlStdd_load_cool / ml_load_cool;
            double rate_load_heat = mlStdd_load_heat / ml_load_heat;
            double rate_load_baseElec = mlStdd_load_baseElec / ml_load_baseElec;

            //query =
            //    $"SELECT " +
            //        $"mnth, " +
            //        $"(load_cool * {rate_load_cool}) as load_cool, " +
            //        $"(load_heat * {rate_load_heat}) as load_heat, " +
            //        $"(load_baseElec * {rate_load_baseElec}) as load_baseElec " +
            //    $"FROM " +
            //        $"tbl_load_energy_usg " +
            //    $"WHERE " +
            //        $"id_etr = {id_etr} AND is_sep = 1;" +

            query =
                // 월별 사용자입력 에너지 (0)
                $"SELECT " +
                    $"* " +
                $"FROM " +
                    $"tbl_load_energy_usg " +
                $"WHERE " +
                    $"id_etr = {id_etr} AND is_sep = 1; " +


                // 월별 참조 사용행태 에너지 (1)
                $"SELECT " +
                    $"mnth, " +
                    $"(load_cool * {rate_load_cool}) as load_cool, " +
                    $"(load_heat * {rate_load_heat}) as load_heat, " +
                    $"(load_baseElec * {rate_load_baseElec}) as load_baseElec " +
                $"FROM " +
                    $"tbl_load_energy_usg " +
                $"WHERE " +
                    $"id_etr = {id_etr} AND is_sep = 1; " +


                // 월별 유사건물군 평균치 에너지 (2)
                $"SELECT " +
                    $"mnth, " +
                    $"AVG(load_cool) as load_cool, " +
                    $"AVG(load_heat) as load_heat, " +
                    $"AVG(load_baseElec) as load_baseElec " +
                $"FROM " +
                    $"tbl_load_energy_usg " +
                $"WHERE " +
                    $"id_etr in " +
                    $"(SELECT " +
                        $"id " +
                    $"FROM " +
                        $"tbl_user_enter " +
                    $"WHERE " +
                        $"area='{area}' AND cd_eqmt='{cd_eqmt}' AND hur_wday={hur_wday} AND hur_wend={hur_wend}) " +
                $"GROUP BY mnth; " +


                // 연간 사용자입력 에너지 (3)
                $"SELECT " +
                    $"SUM(load_cool) as yr_load_cool, " +
                    $"SUM(load_heat) as yr_load_heat, " +
                    $"SUM(load_baseElec) as yr_load_baseElec, " +
                    $"SUM(load_baseGas) as yr_load_baseGas " +
                $"FROM " +
                    $"tbl_load_energy_usg " +
                    $"WHERE " +
                    $"id_etr = {id_etr} AND is_sep = 1; " +


                // 연간 참조 사용행태 에너지 (4)
                $"SELECT " +
                    $"ROUND(SUM(load_cool * {rate_load_cool}), 2) as yr_load_cool, " +
                    $"ROUND(SUM(load_heat * {rate_load_heat}), 2) as yr_load_heat, " +
                    $"ROUND(SUM(load_baseElec * {rate_load_baseElec}), 2) as yr_load_baseElec " +
                $"FROM " +
                    $"tbl_load_energy_usg " +
                $"WHERE " +
                    $"id_etr = {id_etr} AND is_sep = 1; " +


                $"DECLARE @cvtHeat FLOAT, @cvtCool FLOAT, @cvtBC FLOAT " +
                $"IF((SELECT cd_eqmt FROM tbl_user_enter WHERE id = {id_etr}) = 401) " +
                    $"BEGIN " +
                        $"SET @cvtHeat = 0.00046 " +
                        $"SET @cvtCool = 0.00046 " +
                        $"SET @cvtBC = 0.00046 " +
                    $"END " +
                $"ELSE " +
                    $"BEGIN " +
                        $"SET @cvtHeat = 0.000207 " +
                        $"SET @cvtCool = 0.000207 " +
                        $"SET @cvtBC = 0.00046 " +
                    $"END " +


                // 월별 유사건물군 평균치 CO2 (5)
                $"SELECT " +
                    $"mnth, " +
                    $"ROUND(AVG(load_cool)  * @cvtCool, 4) as co2_cool, " +
                    $"ROUND(AVG(load_heat)  * @cvtHeat, 4) as co2_heat, " +
                    $"ROUND(AVG(load_baseElec)  * @cvtBC, 4) as co2_baseElec " +
                $"FROM " +
                    $"tbl_load_energy_usg " +
                $"WHERE " +
                    $"id_etr in " +
                    $"(SELECT " +
                        $"id " +
                    $"FROM " +
                        $"tbl_user_enter " +
                    $"WHERE " +
                        $"area='{area}' AND cd_eqmt='{cd_eqmt}' AND hur_wday={hur_wday} AND hur_wend={hur_wend}) " +
                $"GROUP BY mnth; " +


                // 연간 사용자입력 CO2 (6)
                $"SELECT " +
                    $"ROUND(SUM(load_cool) * @cvtCool, 4) as yr_co2_cool,  " +
                    $"ROUND(SUM(load_heat) * @cvtHeat, 4) as yr_co2_heat,  " +
                    $"ROUND(SUM(load_baseElec) * @cvtBC, 4) as yr_co2_baseElec " +
                $"FROM " +
                    $"tbl_load_energy_usg " +
                $"WHERE  " +
                    $"id_etr = {id_etr} AND is_sep = 1; " +


                // 연간 참조 사용행태 CO2 (7)
                $"SELECT " +
                    $"ROUND(SUM(load_cool * {rate_load_cool}) * @cvtCool, 4) as yr_co2_cool, " +
                    $"ROUND(SUM(load_heat * {rate_load_heat}) * @cvtHeat, 4) as yr_co2_heat, " +
                    $"ROUND(SUM(load_baseElec * {rate_load_baseElec}) * @cvtBC, 4) as yr_co2_baseElec " +
                $"FROM " +
                    $"tbl_load_energy_usg " +
                $"WHERE " +
                    $"id_etr = {id_etr} AND is_sep = 1; ";

            //query = string.Format("SELECT * FROM tbl_load_energy_usg WHERE id_etr={0} AND is_sep=1", id_etr);
            return JsonConvert.SerializeObject(transaction.GetTablesFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("get-energyusage-ml")]
        public string GetEnergyUsageML(string id_etr)
        {
            try
            {
                //string cdEqmt = eqmt == "EHP" ? "401" : "402";

                string dataSource = _configuration.GetConnectionString("PROD");
                string query = string.Format("SELECT * FROM tbl_ml WHERE id_etr={0}", id_etr);
                //string mlJson = JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));

                string stdQuery = string.Format("SELECT * FROM tbl_ml_stdd WHERE id_etr={0}", id_etr);
                //string mlStddJson = JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));

                // ML 예측 ===========
                // mlJson: 사용자입력값 예측 부분, cool, heat, baseElec 3번 실행
                // mlStddJson: 일반사용형태 예측 부분, cool, heat, baseElec 3번 실행
                // 총 6번 예측 실행
                double mlStdd_load_cool = mlController.PredictLoadCool(stdQuery, dataSource);
                double mlStdd_load_heat = mlController.PredictLoadHeat(stdQuery, dataSource);
                double mlStdd_load_baseElec = mlController.PredictLoadBaseElec(stdQuery, dataSource);

                double ml_load_cool = mlController.PredictLoadCool(query, dataSource);
                double ml_load_heat = mlController.PredictLoadHeat(query, dataSource);
                double ml_load_baseElec = mlController.PredictLoadBaseElec(query, dataSource);
                // ==================

                // 용도별 에너지 비율
                double rate_load_cool = mlStdd_load_cool / ml_load_cool;
                double rate_load_heat = mlStdd_load_heat / ml_load_heat;
                double rate_load_baseElec = mlStdd_load_baseElec / ml_load_baseElec;

                query =
                    $"SELECT " +
                        $"mnth, " +
                        $"(load_cool * {rate_load_cool}) as load_cool, " +
                        $"(load_heat * {rate_load_heat}) as load_heat, " +
                        $"(load_baseElec * {rate_load_baseElec}) as load_baseElec " +
                    $"FROM " +
                        $"tbl_load_energy_usg " +
                    $"WHERE " +
                        $"id_etr = {id_etr} AND is_sep = 1;";

                return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
            }
            catch (Exception)
            {
                return "";
            }
        }

        [HttpGet]
        [Route("get-energyusage-avg")]
        public string GetEnergyUsageAvg(string area, string eqmt, string wday, string wend)
        {
            string query = 
                $"SELECT " +
                    $"mnth, " +
                    $"AVG(load_cool) as load_cool, " +
                    $"AVG(load_heat) as load_heat, " +
                    $"AVG(load_baseElec) as load_baseElec " +
                $"FROM " +
                    $"tbl_load_energy_usg " +
                $"WHERE " +
                    $"id_etr in " +
                    $"(SELECT " +
                        $"id " +
                    $"FROM " +
                        $"tbl_user_enter " +
                    $"WHERE " +
                        $"area='{area}' AND cd_eqmt='{eqmt}' AND hur_wday={wday} AND hur_wend={wend}) " +
                $"GROUP BY mnth; ";

            string dataSource = _configuration.GetConnectionString("PROD");
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("get-userenter")]
        public string GetUserEnter(string id_etr)
        {
            string dataSource = _configuration.GetConnectionString("PROD");
            string query = $"SELECT * FROM tbl_user_enter WHERE id=" + id_etr;
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpPost]
        [Route("energytyp")]
        public void PostEnergyType([FromBody] EnergyType energyType)
        {
            var elecJson = Convert.ToString(energyType.elec_data);
            var elecDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(elecJson);

            var gasJson = Convert.ToString(energyType.gas_data);
            var gasDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(gasJson);

            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = _configuration.GetConnectionString("PROD");
                    connection.Open();

                    string query = @"INSERT INTO tbl_load_energy_typ VALUES" +
                        "(@id_etr, @mnth, @load_gas, @load_elec, @unit_gas, @unit_elec)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@id_etr", SqlDbType.NVarChar);
                        command.Parameters.Add("@mnth", SqlDbType.NVarChar);
                        command.Parameters.Add("@load_gas", SqlDbType.NVarChar);
                        command.Parameters.Add("@load_elec", SqlDbType.NVarChar);
                        command.Parameters.Add("@unit_gas", SqlDbType.NVarChar);
                        command.Parameters.Add("@unit_elec", SqlDbType.NVarChar);

                        int month = 1;

                        foreach (var elec in elecDict)
                        {
                            command.Parameters["@id_etr"].Value = (object)energyType.id_etr ?? DBNull.Value;
                            command.Parameters["@mnth"].Value = month;
                            command.Parameters["@load_gas"].Value = gasDict[elec.Key];
                            command.Parameters["@load_elec"].Value = elec.Value;
                            command.Parameters["@unit_gas"].Value = (object)energyType.unit_gas ?? DBNull.Value;
                            command.Parameters["@unit_elec"].Value = (object)energyType.unit_elec ?? DBNull.Value;

                            command.ExecuteNonQuery();
                            month++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        [HttpPost]
        [Route("energyusage")]
        public void PostEnergyUsage([FromBody] EnergyUsage energyUsage)
        {
            var elecJson = Convert.ToString(energyUsage.elec_data);
            var elecDict = JsonConvert.DeserializeObject<Dictionary<string, double>>(elecJson);

            var gasJson = Convert.ToString(energyUsage.gas_data);
            var gasDict = JsonConvert.DeserializeObject<Dictionary<string, double>>(gasJson);

            double gasConvert = 0;
            if (energyUsage.unit_gas == 201)
            {
                gasConvert = 0.2777777778;
            }
            else
            {
                gasConvert = 10.55;
            }

            Dictionary<string, double> energyDict;

            if (energyUsage.is_ehp)
            {
                foreach (var elec in elecDict)
                {
                    elecDict[elec.Key] = elec.Value - energyUsage.base_ec;
                }
                energyDict = elecDict;
            }
            else
            {

                foreach (var gas in gasDict)
                {
                    gasDict[gas.Key] = (gas.Value - energyUsage.base_ec) * gasConvert;
                }
                energyDict = gasDict;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = _configuration.GetConnectionString("PROD");
                    connection.Open();

                    string query = @"INSERT INTO tbl_load_energy_usg VALUES" +
                        "(@id_etr, @is_sep, @mnth, @load_cool, @load_heat, @unit_cool, @unit_heat, @load_baseElec, @load_baseGas, @unit_baseElec, @unit_baseGas)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@id_etr", SqlDbType.NVarChar);
                        command.Parameters.Add("@is_sep", SqlDbType.NVarChar);
                        command.Parameters.Add("@mnth", SqlDbType.NVarChar);
                        command.Parameters.Add("@load_cool", SqlDbType.NVarChar);
                        command.Parameters.Add("@load_heat", SqlDbType.NVarChar);
                        command.Parameters.Add("@unit_cool", SqlDbType.NVarChar);
                        command.Parameters.Add("@unit_heat", SqlDbType.NVarChar);
                        command.Parameters.Add("@load_baseElec", SqlDbType.NVarChar);
                        command.Parameters.Add("@load_baseGas", SqlDbType.NVarChar);
                        command.Parameters.Add("@unit_baseElec", SqlDbType.NVarChar);
                        command.Parameters.Add("@unit_baseGas", SqlDbType.NVarChar);

                        int month = 1;

                        foreach (var energy in energyDict)
                        {
                            command.Parameters["@id_etr"].Value = (object)energyUsage.id_etr ?? DBNull.Value;
                            command.Parameters["@is_sep"].Value = 1;
                            command.Parameters["@mnth"].Value = month;

                            if (month > 4 && month < 11)
                            {
                                command.Parameters["@load_cool"].Value = energy.Value;
                                command.Parameters["@load_heat"].Value = 0;

                            }
                            else
                            {
                                command.Parameters["@load_cool"].Value = 0;
                                command.Parameters["@load_heat"].Value = energy.Value;
                            }

                            if (energyUsage.is_ehp)
                            {
                                command.Parameters["@load_baseElec"].Value = energyUsage.base_ec;
                                command.Parameters["@load_baseGas"].Value = gasDict[energy.Key] * gasConvert;
                                command.Parameters["@unit_cool"].Value = (object)energyUsage.unit_elec ?? DBNull.Value;
                                command.Parameters["@unit_heat"].Value = (object)energyUsage.unit_elec ?? DBNull.Value;
                            }
                            else
                            {
                                command.Parameters["@load_baseElec"].Value = elecDict[energy.Key];
                                command.Parameters["@load_baseGas"].Value = energyUsage.base_ec * gasConvert;
                                command.Parameters["@unit_cool"].Value = (object)energyUsage.unit_gas ?? DBNull.Value;
                                command.Parameters["@unit_heat"].Value = (object)energyUsage.unit_gas ?? DBNull.Value;
                            }

                            command.Parameters["@unit_baseElec"].Value = (object)energyUsage.unit_elec ?? DBNull.Value;
                            command.Parameters["@unit_baseGas"].Value = (object)energyUsage.unit_gas ?? DBNull.Value;

                            command.ExecuteNonQuery();
                            month++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [HttpPost]
        [Route("userenter")]
        public int PostUserEnter([FromBody] UserEnter userEnter)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = _configuration.GetConnectionString("PROD");
                    connection.Open();

                    //string body = Request.Content.ReadAsStringAsync().Result;

                    //string test = Convert.ToString(obj);
                    string query = @"INSERT INTO tbl_user_enter OUTPUT INSERTED.id VALUES" +
                    "(@address, @cd_north_axis, @cd_usage_main, @usage_sub, @year, @area, @wwr, " +
                    "@isetr_wwr, @aspect_ratio, @isetr_aspect_ratio, @u_wall, @u_roof, @u_floor, " +
                    "@u_window, @shgc, @cd_eqmt, @effcy_heat, @effcy_cool, @cd_eqmt_light, @level_light, " +
                    "@isetr_light, @hur_wday, @hur_wend, @men_rsdt, @men_norsdt, @temp_heat, @temp_cool, " +
                    "@cd_unitgas, @isetr_u_wall, @isetr_u_roof, @isetr_u_floor, @isetr_u_window, @isetr_shgc, " +
                    "@area_etr, @dt_create)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@address", SqlDbType.NVarChar);
                        command.Parameters.Add("@cd_north_axis", SqlDbType.NVarChar);
                        command.Parameters.Add("@cd_usage_main", SqlDbType.NVarChar);
                        command.Parameters.Add("@usage_sub", SqlDbType.NVarChar);
                        command.Parameters.Add("@year", SqlDbType.NVarChar);
                        command.Parameters.Add("@area", SqlDbType.NVarChar);
                        command.Parameters.Add("@wwr", SqlDbType.NVarChar);
                        command.Parameters.Add("@isetr_wwr", SqlDbType.NVarChar);
                        command.Parameters.Add("@aspect_ratio", SqlDbType.NVarChar);
                        command.Parameters.Add("@isetr_aspect_ratio", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_wall", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_roof", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_floor", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_window", SqlDbType.NVarChar);
                        command.Parameters.Add("@shgc", SqlDbType.NVarChar);
                        command.Parameters.Add("@cd_eqmt", SqlDbType.NVarChar);
                        command.Parameters.Add("@effcy_heat", SqlDbType.NVarChar);
                        command.Parameters.Add("@effcy_cool", SqlDbType.NVarChar);
                        command.Parameters.Add("@cd_eqmt_light", SqlDbType.NVarChar);
                        command.Parameters.Add("@level_light", SqlDbType.NVarChar);
                        command.Parameters.Add("@isetr_light", SqlDbType.NVarChar);
                        command.Parameters.Add("@hur_wday", SqlDbType.NVarChar);
                        command.Parameters.Add("@hur_wend", SqlDbType.NVarChar);
                        command.Parameters.Add("@men_rsdt", SqlDbType.NVarChar);
                        command.Parameters.Add("@men_norsdt", SqlDbType.NVarChar);
                        command.Parameters.Add("@temp_heat", SqlDbType.NVarChar);
                        command.Parameters.Add("@temp_cool", SqlDbType.NVarChar);
                        command.Parameters.Add("@cd_unitgas", SqlDbType.NVarChar);
                        command.Parameters.Add("@isetr_u_wall", SqlDbType.NVarChar);
                        command.Parameters.Add("@isetr_u_roof", SqlDbType.NVarChar);
                        command.Parameters.Add("@isetr_u_floor", SqlDbType.NVarChar);
                        command.Parameters.Add("@isetr_u_window", SqlDbType.NVarChar);
                        command.Parameters.Add("@isetr_shgc", SqlDbType.NVarChar);
                        command.Parameters.Add("@area_etr", SqlDbType.NVarChar);
                        command.Parameters.Add("@dt_create", SqlDbType.NVarChar);

                        command.Parameters["@address"].Value = (object)userEnter.address ?? DBNull.Value;
                        command.Parameters["@cd_north_axis"].Value = (object)userEnter.cd_north_axis ?? DBNull.Value;
                        command.Parameters["@cd_usage_main"].Value = (object)userEnter.cd_usage_main ?? DBNull.Value;
                        command.Parameters["@usage_sub"].Value = (object)userEnter.usage_sub ?? DBNull.Value;
                        command.Parameters["@year"].Value = (object)userEnter.year ?? DBNull.Value;
                        command.Parameters["@area"].Value = (object)userEnter.area ?? DBNull.Value;
                        command.Parameters["@wwr"].Value = (object)userEnter.wwr ?? DBNull.Value;
                        command.Parameters["@isetr_wwr"].Value = (object)userEnter.isetr_wwr ?? DBNull.Value;
                        command.Parameters["@aspect_ratio"].Value = (object)userEnter.aspect_ratio ?? DBNull.Value;
                        command.Parameters["@isetr_aspect_ratio"].Value = (object)userEnter.isetr_aspect_ratio ?? DBNull.Value;
                        command.Parameters["@u_wall"].Value = (object)userEnter.u_wall ?? DBNull.Value;
                        command.Parameters["@u_roof"].Value = (object)userEnter.u_roof ?? DBNull.Value;
                        command.Parameters["@u_floor"].Value = (object)userEnter.u_floor ?? DBNull.Value;
                        command.Parameters["@u_window"].Value = (object)userEnter.u_window ?? DBNull.Value;
                        command.Parameters["@shgc"].Value = (object)userEnter.shgc ?? DBNull.Value;
                        command.Parameters["@cd_eqmt"].Value = (object)userEnter.cd_eqmt ?? DBNull.Value;
                        command.Parameters["@effcy_heat"].Value = (object)userEnter.effcy_heat ?? DBNull.Value;
                        command.Parameters["@effcy_cool"].Value = (object)userEnter.effcy_cool ?? DBNull.Value;
                        command.Parameters["@cd_eqmt_light"].Value = (object)userEnter.cd_eqmt_light ?? DBNull.Value;
                        command.Parameters["@level_light"].Value = (object)userEnter.level_light ?? DBNull.Value;
                        command.Parameters["@isetr_light"].Value = (object)userEnter.isetr_light ?? DBNull.Value;
                        command.Parameters["@hur_wday"].Value = (object)userEnter.hur_wday ?? DBNull.Value;
                        command.Parameters["@hur_wend"].Value = (object)userEnter.hur_wend ?? DBNull.Value;
                        command.Parameters["@men_rsdt"].Value = (object)userEnter.men_rsdt ?? DBNull.Value;
                        command.Parameters["@men_norsdt"].Value = (object)userEnter.men_norsdt ?? DBNull.Value;
                        command.Parameters["@temp_heat"].Value = (object)userEnter.temp_heat ?? DBNull.Value;
                        command.Parameters["@temp_cool"].Value = (object)userEnter.temp_cool ?? DBNull.Value;
                        command.Parameters["@cd_unitgas"].Value = (object)userEnter.cd_unitgas ?? DBNull.Value;
                        command.Parameters["@isetr_u_wall"].Value = (object)userEnter.isetr_u_wall ?? DBNull.Value;
                        command.Parameters["@isetr_u_roof"].Value = (object)userEnter.isetr_u_roof ?? DBNull.Value;
                        command.Parameters["@isetr_u_floor"].Value = (object)userEnter.isetr_u_floor ?? DBNull.Value;
                        command.Parameters["@isetr_u_window"].Value = (object)userEnter.isetr_u_window ?? DBNull.Value;
                        command.Parameters["@isetr_shgc"].Value = (object)userEnter.isetr_shgc ?? DBNull.Value;
                        command.Parameters["@area_etr"].Value = (object)userEnter.area_etr ?? DBNull.Value;
                        command.Parameters["@dt_create"].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));

                        return (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        [HttpPost]
        [Route("ml")]
        public string PostML([FromBody] ML ml)
        {
            try
            {
                string dataSource = _configuration.GetConnectionString("PROD");

                string query = $"SELECT * FROM tbl_value_range";
                var dtMinMax = transaction.GetTableFromDB(query, dataSource);

                ml.SetValueRange(dtMinMax);

                query = $"SELECT value FROM tbl_com_code WHERE code = '{(object)ml.cd_north_axis}'";
                var dt = transaction.GetTableFromDB(query, dataSource);
                var north_axis = dt.Rows[0]["value"];

                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = _configuration.GetConnectionString("PROD");
                    connection.Open();

                    query =
                        $"INSERT INTO tbl_ml" +
                        $"(id_etr, [year], eqmt, area, aspect_ratio, temp_cool, pwr_eqmt, temp_heat, level_light, north_axis, occupancy, shgc, u_floor, u_roof, u_wall, u_window, hur_wday, hur_wend, wwr, effcy_cool, effcy_heat) " +
                        $"OUTPUT INSERTED.id " +
                        $"VALUES" +
                        $"(@id_etr, " +
                        $"@year, " +
                        $"@eqmt, " +
                        $"@area, " +
                        $"@aspect_ratio, " +
                        $"@temp_cool, " +
                        $"@pwr_eqmt, " +
                        $"@temp_heat, " +
                        $"@level_light, " +
                        $"@north_axis, " +
                        $"@occupancy, " +
                        $"@shgc, " +
                        $"@u_floor, " +
                        $"@u_roof, " +
                        $"@u_wall, " +
                        $"@u_window, " +
                        $"@hur_wday, " +
                        $"@hur_wend, " +
                        $"@wwr, " +
                        $"@effcy_cool, " +
                        $"@effcy_heat)";

                    int id_ml = 0;
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@id_etr", SqlDbType.NVarChar);
                        command.Parameters.Add("@year", SqlDbType.NVarChar);
                        command.Parameters.Add("@eqmt", SqlDbType.NVarChar);
                        command.Parameters.Add("@area", SqlDbType.NVarChar);
                        command.Parameters.Add("@aspect_ratio", SqlDbType.NVarChar);
                        command.Parameters.Add("@temp_cool", SqlDbType.NVarChar);
                        command.Parameters.Add("@pwr_eqmt", SqlDbType.NVarChar);
                        command.Parameters.Add("@temp_heat", SqlDbType.NVarChar);
                        command.Parameters.Add("@level_light", SqlDbType.NVarChar);
                        command.Parameters.Add("@north_axis", SqlDbType.NVarChar);
                        command.Parameters.Add("@occupancy", SqlDbType.NVarChar);
                        command.Parameters.Add("@shgc", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_floor", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_roof", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_wall", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_window", SqlDbType.NVarChar);
                        command.Parameters.Add("@hur_wday", SqlDbType.NVarChar);
                        command.Parameters.Add("@hur_wend", SqlDbType.NVarChar);
                        command.Parameters.Add("@wwr", SqlDbType.NVarChar);
                        command.Parameters.Add("@effcy_cool", SqlDbType.NVarChar);
                        command.Parameters.Add("@effcy_heat", SqlDbType.NVarChar);

                        command.Parameters["@id_etr"].Value = (object)ml.id_etr ?? DBNull.Value;
                        command.Parameters["@year"].Value = (object)ml.year ?? DBNull.Value;
                        command.Parameters["@eqmt"].Value = (object)ml.cd_eqmt ?? DBNull.Value;
                        command.Parameters["@area"].Value = (object)ml.area ?? DBNull.Value;
                        command.Parameters["@aspect_ratio"].Value = (object)ml.aspect_ratio ?? DBNull.Value;
                        command.Parameters["@temp_cool"].Value = (object)ml.temp_cool ?? DBNull.Value;
                        command.Parameters["@pwr_eqmt"].Value = (object)ml.pwr_eqmt ?? DBNull.Value;
                        command.Parameters["@temp_heat"].Value = (object)ml.temp_heat ?? DBNull.Value;
                        command.Parameters["@level_light"].Value = (object)ml.level_light ?? DBNull.Value;
                        command.Parameters["@north_axis"].Value = north_axis ?? DBNull.Value;
                        command.Parameters["@occupancy"].Value = (object)ml.occupancy ?? DBNull.Value;
                        command.Parameters["@shgc"].Value = (object)ml.shgc ?? DBNull.Value;
                        command.Parameters["@u_floor"].Value = (object)ml.u_floor ?? DBNull.Value;
                        command.Parameters["@u_roof"].Value = (object)ml.u_roof ?? DBNull.Value;
                        command.Parameters["@u_wall"].Value = (object)ml.u_wall ?? DBNull.Value;
                        command.Parameters["@u_window"].Value = (object)ml.u_window ?? DBNull.Value;
                        command.Parameters["@hur_wday"].Value = (object)ml.hur_wday ?? DBNull.Value;
                        command.Parameters["@hur_wend"].Value = (object)ml.hur_wend ?? DBNull.Value;
                        command.Parameters["@wwr"].Value = (object)ml.wwr ?? DBNull.Value;
                        command.Parameters["@effcy_cool"].Value = (object)ml.effcy_cool ?? DBNull.Value;
                        command.Parameters["@effcy_heat"].Value = (object)ml.effcy_heat ?? DBNull.Value;

                        id_ml = (int)command.ExecuteScalar();
                    }

                    query =
                        $"INSERT INTO tbl_ml_stdd" +
                        $"(id_etr, [year], eqmt, area, aspect_ratio, temp_cool, pwr_eqmt, temp_heat, level_light, north_axis, occupancy, shgc, u_floor, u_roof, u_wall, u_window, hur_wday, hur_wend, wwr, effcy_cool, effcy_heat) " +
                        $"OUTPUT INSERTED.id " +
                        $"VALUES" +
                        $"(@id_etr, " +
                        $"@year, " +
                        $"@eqmt, " +
                        $"@area, " +
                        $"@aspect_ratio, " +
                        $"@temp_cool, " +
                        $"@pwr_eqmt, " +
                        $"@temp_heat, " +
                        $"@level_light, " +
                        $"@north_axis, " +
                        $"@occupancy, " +
                        $"@shgc, " +
                        $"@u_floor, " +
                        $"@u_roof, " +
                        $"@u_wall, " +
                        $"@u_window, " +
                        $"@hur_wday, " +
                        $"@hur_wend, " +
                        $"@wwr, " +
                        $"@effcy_cool, " +
                        $"@effcy_heat)";

                    int id_ml_stdd = 0;

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@id_etr", SqlDbType.NVarChar);
                        command.Parameters.Add("@year", SqlDbType.NVarChar);
                        command.Parameters.Add("@eqmt", SqlDbType.NVarChar);
                        command.Parameters.Add("@area", SqlDbType.NVarChar);
                        command.Parameters.Add("@aspect_ratio", SqlDbType.NVarChar);
                        command.Parameters.Add("@temp_cool", SqlDbType.NVarChar);
                        command.Parameters.Add("@pwr_eqmt", SqlDbType.NVarChar);
                        command.Parameters.Add("@temp_heat", SqlDbType.NVarChar);
                        command.Parameters.Add("@level_light", SqlDbType.NVarChar);
                        command.Parameters.Add("@north_axis", SqlDbType.NVarChar);
                        command.Parameters.Add("@occupancy", SqlDbType.NVarChar);
                        command.Parameters.Add("@shgc", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_floor", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_roof", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_wall", SqlDbType.NVarChar);
                        command.Parameters.Add("@u_window", SqlDbType.NVarChar);
                        command.Parameters.Add("@hur_wday", SqlDbType.NVarChar);
                        command.Parameters.Add("@hur_wend", SqlDbType.NVarChar);
                        command.Parameters.Add("@wwr", SqlDbType.NVarChar);
                        command.Parameters.Add("@effcy_cool", SqlDbType.NVarChar);
                        command.Parameters.Add("@effcy_heat", SqlDbType.NVarChar);

                        command.Parameters["@id_etr"].Value = (object)ml.id_etr ?? DBNull.Value;
                        command.Parameters["@year"].Value = (object)ml.year ?? DBNull.Value;
                        command.Parameters["@eqmt"].Value = (object)ml.cd_eqmt ?? DBNull.Value;
                        command.Parameters["@area"].Value = (object)ml.area ?? DBNull.Value;
                        command.Parameters["@aspect_ratio"].Value = (object)ml.aspect_ratio ?? DBNull.Value;
                        command.Parameters["@temp_cool"].Value = 28;
                        command.Parameters["@pwr_eqmt"].Value = (object)ml.pwr_eqmt ?? DBNull.Value;
                        command.Parameters["@temp_heat"].Value = 18;
                        command.Parameters["@level_light"].Value = (object)ml.level_light ?? DBNull.Value;
                        command.Parameters["@north_axis"].Value = north_axis ?? DBNull.Value;
                        command.Parameters["@occupancy"].Value = (object)ml.occupancy ?? DBNull.Value;
                        command.Parameters["@shgc"].Value = (object)ml.shgc ?? DBNull.Value;
                        command.Parameters["@u_floor"].Value = (object)ml.u_floor ?? DBNull.Value;
                        command.Parameters["@u_roof"].Value = (object)ml.u_roof ?? DBNull.Value;
                        command.Parameters["@u_wall"].Value = (object)ml.u_wall ?? DBNull.Value;
                        command.Parameters["@u_window"].Value = (object)ml.u_window ?? DBNull.Value;
                        command.Parameters["@hur_wday"].Value = 8;
                        command.Parameters["@hur_wend"].Value = 0;
                        command.Parameters["@wwr"].Value = (object)ml.wwr ?? DBNull.Value;
                        command.Parameters["@effcy_cool"].Value = (object)ml.effcy_cool ?? DBNull.Value;
                        command.Parameters["@effcy_heat"].Value = (object)ml.effcy_heat ?? DBNull.Value;

                        id_ml_stdd = (int)command.ExecuteScalar();
                    }

                    var json = new { id_ml = id_ml, id_ml_stdd = id_ml_stdd };
                    string jsonString = JsonConvert.SerializeObject(json);
                    return jsonString;
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        [HttpPost]
        [Route("train-ml")]
        public string TrainML([FromBody] MLTrain mlTrain)
        {
            try
            {
                EnergyTrainingController training = new EnergyTrainingController();
                var result = training.Train(mlTrain.area, mlTrain.eqmt, mlTrain.type);
                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "ML Train Failed";
            }
        }

        
    }

    public class ETR
    {
        public int id_etr { get; set; }
    }

    public class UserEnter
    {
        public string address { get; set; }
        public float cd_north_axis { get; set; }
        public int cd_usage_main { get; set; }
        public string usage_sub { get; set; }
        public int year { get; set; }
        public float area { get; set; }
        public float wwr { get; set; }
        public int isetr_wwr { get; set; }
        public float aspect_ratio { get; set; }
        public int isetr_aspect_ratio { get; set; }
        public float area_etr { get; set; }
        public string u_wall { get; set; }
        public string u_roof { get; set; }
        public string u_floor { get; set; }
        public string u_window { get; set; }
        public string shgc { get; set; }
        public int cd_eqmt { get; set; }
        public float effcy_heat { get; set; }
        public float effcy_cool { get; set; }
        public int cd_eqmt_light { get; set; }
        public float level_light { get; set; }
        public int isetr_light { get; set; }
        public int isetr_u_wall { get; set; }
        public int isetr_u_roof { get; set; }
        public int isetr_u_floor { get; set; }
        public int isetr_u_window { get; set; }
        public int isetr_shgc { get; set; }
        public string hur_wday { get; set; }
        public string hur_wend { get; set; }
        public float men_rsdt { get; set; }
        public float men_norsdt { get; set; }
        public float temp_heat { get; set; }
        public float temp_cool { get; set; }
        public int cd_unitgas { get; set; }
    }

    public class EnergyType
    {
        public int id_etr { get; set; }
        public int unit_elec { get; set; }
        public int unit_gas { get; set; }
        public dynamic elec_data { get; set; }
        public dynamic gas_data { get; set; }
    }

    public class EnergyUsage
    {
        public int id_etr { get; set; }
        public int base_ec { get; set; }
        public int unit_elec { get; set; }
        public int unit_gas { get; set; }
        public dynamic elec_data { get; set; }
        public dynamic gas_data { get; set; }
        public bool is_ehp { get; set; }
    }

    public class ML
    {
        public int id_etr { get; set; }
        public int cd_eqmt { get; set; }
        public int year { get; set; }
        public int area { get; set; }
        public float load_cool { get; set; }
        public float load_heat { get; set; }
        public float load_baseElec { get; set; }
        public float aspect_ratio { get; set; }
        public float temp_cool { get; set; }//
        public float pwr_eqmt { get; set; }//
        public float temp_heat { get; set; }//
        public float level_light { get; set; }//
        public int cd_north_axis { get; set; }
        public float occupancy { get; set; }//
        public float shgc { get; set; }//
        public float u_floor { get; set; }//
        public float u_roof { get; set; }//
        public float u_wall { get; set; }//
        public float u_window { get; set; }//
        public int hur_wday { get; set; }
        public int hur_wend { get; set; }
        public float wwr { get; set; }
        public float effcy_cool { get; set; }//
        public float effcy_heat { get; set; }//

        public void SetValueRange(DataTable dt)
        {
            temp_cool = SetMinMax(temp_cool, "temp_cool", dt);
            pwr_eqmt = SetMinMax(pwr_eqmt, "pwr_eqmt", dt);
            temp_heat = SetMinMax(temp_heat, "temp_heat", dt);
            level_light = SetMinMax(level_light, "level_light", dt);
            occupancy = SetMinMax(occupancy, "occupancy", dt);
            shgc = SetMinMax(shgc, "shgc", dt);
            u_floor = SetMinMax(u_floor, "u_floor", dt);
            u_roof = SetMinMax(u_roof, "u_roof", dt);
            u_wall = SetMinMax(u_wall, "u_wall", dt);
            u_window = SetMinMax(u_window, "u_window", dt);

            if (cd_eqmt == 401)
            {
                effcy_cool = SetMinMax(effcy_cool, "effcy_cool_ehp", dt);
                effcy_heat = SetMinMax(effcy_heat, "effcy_heat_ehp", dt);
            }
            else
            {
                effcy_cool = SetMinMax(effcy_cool, "effcy_cool_ctr", dt);
                effcy_heat = SetMinMax(effcy_heat, "effcy_heat_ctr", dt);
            }
        }
        private float SetMinMax(float value, string name, DataTable dt)
        {
            string expression = $"name = '{name}'";
            DataRow? dtRow = dt.Select(expression).FirstOrDefault();

            double min = Convert.ToDouble(dtRow?["min_val"]);
            double max = Convert.ToDouble(dtRow?["max_val"]);

            float val2 = value;
            if (value > max)
            {
                val2 = (float)max;
            }
            else if (value < min)
            {
                val2 = (float)min;
            }

            return val2;
        }
    }

    public class MLTrain
    {
        public string area { get; set; }
        public string eqmt { get; set; }
        public string type { get; set; }
    }
}
