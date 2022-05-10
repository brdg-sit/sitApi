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
        [Route("ml")]
        public string GetMlData()
        {
            string query = @"SELECT * FROM tbl_ml";

            string dataSource = _configuration.GetConnectionString("DevServerConnectionString");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("elements")]
        public string GetProjects()
        {
            string query = @"SELECT * FROM tbl_com_code";

            string dataSource = _configuration.GetConnectionString("DevServerConnectionString");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }
    }
}
