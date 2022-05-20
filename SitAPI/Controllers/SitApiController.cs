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

namespace UnrealViewerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Sit : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private Transaction transaction = new Transaction();

        public Sit(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        [HttpGet]
        [Route("mldata")]
        public string GetMlData()
        {
            string query = @"SELECT * FROM tbl_ml";

            string dataSource = _configuration.GetConnectionString("MSSQLServerConnectionString");

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
                    $"(select name from tbl_com_code where code = A.cd_eqmt_heat) as cd_eqmt_heat, " +
                    $"(select name from tbl_com_code where code = A.cd_eqmt_cool) as cd_eqmt_cool, " +
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

            string dataSource = _configuration.GetConnectionString("MSSQLServerConnectionString");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("tableinfo")]
        public string GetProjects(string nmTable)
        {
            string query = $"SELECT name as field, label as headerName, editable FROM tbl_def_cols WHERE [table] = '{nmTable}' order by ORDER_SORT";

            string dataSource = _configuration.GetConnectionString("MSSQLServerConnectionString");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        //[HttpOptions]
        //[Route("Books", Name = "Options")]
        //public IActionResult Options()
        //{
        //    //HttpListenerResponse.AppendHeader("Allow", "GET,OPTIONS");
        //    HttpContext.Response.Headers.Add("Allow", "GET, OPTIONS");
        //    return Ok();
        //}


        //[HttpOptions]
        //[Route("load")]
        //public IActionResult PreflightRoute()
        //{
        //    return NoContent();
        //}

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
                    $"* " +
                $"FROM " +
                    $"tbl_load_energy_typ A " +
                $"WHERE " +
                    $"A.id_etr = {ID}";

            string dataSource = _configuration.GetConnectionString("MSSQLServerConnectionString");

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
                    $"(select name from tbl_com_code where code = A.unit_cool) as unit_cool, " +
                    $"A.load_heat, " +
                    $"(select name from tbl_com_code where code = A.unit_cool) as unit_heat, " +
                    $"A.load_baseElec, " +
                    $"(select name from tbl_com_code where code = A.unit_cool) as unit_baseElec, " +
                    $"A.load_baseGas, " +
                    $"(select name from tbl_com_code where code = A.unit_cool) as unit_baseGasc " +
                $"FROM " +
                    $"tbl_load_energy_usg A " +
                $"WHERE " +
                    $"A.id_etr = {ID} AND A.is_sep = 1";

            string dataSource = _configuration.GetConnectionString("MSSQLServerConnectionString");

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

                string dataSource = _configuration.GetConnectionString("MSSQLServerConnectionString");
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

                string dataSource = _configuration.GetConnectionString("MSSQLServerConnectionString");

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
    }
}
