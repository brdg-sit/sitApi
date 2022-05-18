﻿using Microsoft.AspNetCore.Http;
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
            string query = @"SELECT * FROM tbl_user_enter";

            string dataSource = _configuration.GetConnectionString("MSSQLServerConnectionString");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("tableinfo")]
        public string GetProjects(string nmTable)
        {
            string query = $"SELECT name as field, label as headerName FROM tbl_def_cols WHERE [table] = '{nmTable}' order by ORDER_SORT";

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
        [Route("load")]
        public string GetLoads(object obj)
        {
            JObject? jobj = JObject.Parse(obj.ToString());
            if (jobj.Count == 0)
            {
                return "";
            }

            var ID = jobj["id"];

            string query =
                $"SELECT * " +
                $"FROM " +
                $"tbl_load_energy_typ A " +
                $"WHERE " +
                $"A.id_etr = {ID}";

            string dataSource = _configuration.GetConnectionString("MSSQLServerConnectionString");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

    }
}
