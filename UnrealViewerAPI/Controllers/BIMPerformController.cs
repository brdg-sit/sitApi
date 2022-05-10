using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Web;
using System;
using Microsoft.Web.Administration;
using System.Net;
using System.Transactions;

namespace UnrealViewerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BIMPerformController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private Transaction transaction = new Transaction();

        public BIMPerformController(IConfiguration configuration)
        {
            this._configuration = configuration;
        }




        [HttpGet]
        [Route("elements")]
        public string GetProjects(int employeeId, string startDate, string endDate)
        {
            string query = string.Format(
                @"SELECT project_code, project_name, occurred_on FROM TB_PERFORMANCE_ELEMENT_LOG WHERE id IN (SELECT MAX(id) FROM TB_PERFORMANCE_ELEMENT_LOG WHERE project_code is not null AND project_code != 'RFA' AND employee_id = '{0}'
                AND occurred_on >= '{1}' AND occurred_on < DATEADD(DAY ,1,'{2}') GROUP BY project_code) ORDER BY occurred_on DESC", employeeId, startDate, endDate);

            string dataSource = _configuration.GetConnectionString("DevServerConnectionString");

            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));

            //return new JsonResult(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("models")]
        public string GetModels(int employeeId, string startDate, string endDate, string projectCode)
        {
            string query = string.Format(
                @"SELECT category_type, category_name
                FROM TB_PERFORMANCE_ELEMENT_LOG
                WHERE employee_id='{0}' AND project_code='{1}' AND category_type='Model' 
                AND occurred_on >= '{2}' AND occurred_on < DATEADD(DAY ,1,'{3}')", employeeId, projectCode, startDate, endDate);

            string dataSource = _configuration.GetConnectionString("DevServerConnectionString");
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("annotations")]
        public string GetAnnotations(int employeeId, string startDate, string endDate, string projectCode)
        {
            string query = string.Format(
                @"SELECT category_type, category_name
                FROM TB_PERFORMANCE_ELEMENT_LOG
                WHERE employee_id='{0}' AND project_code='{1}' AND category_type='Annotation'
                AND occurred_on >= '{2}' AND occurred_on < DATEADD(DAY ,1,'{3}')", employeeId, projectCode, startDate, endDate);

            string dataSource = _configuration.GetConnectionString("DevServerConnectionString");
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("transactionCountPerDay")]
        public string GetTransactionCountPerDay(string startDate, string endDate, string projectCode, int? employeeId)
        {
            string query;

            //For Team Transactions
            if (employeeId == null)
            {
                query = string.Format(
                    @"SELECT DATEPART(DAY, occurred_on) as Day, DATEPART(MONTH, occurred_on) as Month, DATEPART(YEAR, occurred_on) as Year, COUNT(*) AS TransactionCount
                        FROM TB_PERFORMANCE_ELEMENT_LOG
                        WHERE occurred_on >= '{0}' AND occurred_on < DATEADD(DAY ,1,'{1}') AND project_code = '{2}' AND (category_type = 'model' OR category_type = 'Annotation')
                        GROUP BY DATEPART(DAY, occurred_on),  DATEPART(MONTH, occurred_on), DATEPART(YEAR, occurred_on) ORDER BY Month ASC, Day;", startDate, endDate, projectCode);
            }
            //For Personal Transactions
            else
            {
                query = string.Format(
                    @"SELECT DATEPART(DAY, occurred_on) as Day, DATEPART(MONTH, occurred_on) as Month, DATEPART(YEAR, occurred_on) as Year, COUNT(*) AS TransactionCount
                        FROM TB_PERFORMANCE_ELEMENT_LOG 
                        WHERE occurred_on >= '{0}' AND occurred_on < DATEADD(DAY ,1,'{1}') AND project_code = '{2}' AND employee_id='{3}' AND (category_type = 'model' OR category_type = 'Annotation')
                        GROUP BY DATEPART(DAY, occurred_on),  DATEPART(MONTH, occurred_on), DATEPART(YEAR, occurred_on) ORDER BY Month ASC, Day;", startDate, endDate, projectCode, employeeId);
            }

            string dataSource = _configuration.GetConnectionString("DevServerConnectionString");
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }
        [HttpGet]
        [Route("warnings")]
        public string GetWarnings(string projectCode)
        {
            string query = string.Format(@"SELECT description, severity, element_ids FROM TB_PERFORMANCE_WARNING WHERE project_code='{0}'", projectCode);
            string dataSource = _configuration.GetConnectionString("DevServerConnectionString");
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("participatingEmployees")]
        public string GetParticipatingEmployees(string startDate, string endDate, string projectCode)
        {
            string query = string.Format(
                @"SELECT DATEPART(DAY, occurred_on) AS Day, DATEPART(MONTH, occurred_on) AS Month, DATEPART(YEAR, occurred_on) AS Year, employee_id AS employeeId
FROM[dbo].[TB_PERFORMANCE_ELEMENT_LOG]
WHERE occurred_on >= '{1}' AND occurred_on < DATEADD(DAY, 1, '{2}') AND project_code = '{3}' AND(category_type = 'model' OR category_type = 'Annotation') AND employee_id IS NOT NULL
GROUP BY DATEPART(DAY, occurred_on), DATEPART(MONTH, occurred_on), DATEPART(YEAR, occurred_on), employee_id ORDER BY Month ASC, Day;", "TB_PERFORMANCE_ELEMENT_LOG", startDate, endDate, projectCode);

            string dataSource = _configuration.GetConnectionString("DevServerConnectionString");
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }

        [HttpGet]
        [Route("totalTransactionCount")]
        public string GetTotalTransactionCount(string startDate, string endDate, string projectCode, int? employeeId)
        {
            string query = "";
            //For Team Transactions
            if (employeeId == null)
            {
                query = string.Format(
                @"SELECT COUNT(*) AS totalTransactionCount
                FROM {0}
                WHERE occurred_on >= '{1}' AND occurred_on < DATEADD(DAY ,1,'{2}')
                AND project_code = '{3}' AND (category_type = 'model' OR category_type = 'Annotation')", "TB_PERFORMANCE_ELEMENT_LOG", startDate, endDate, projectCode);
            }
            else
            {
                query = string.Format(
                @"SELECT COUNT(*) AS totalTransactionCount
                FROM {0}
                WHERE occurred_on >= '{1}' AND occurred_on < DATEADD(DAY ,1,'{2}') AND project_code = '{3}'
                AND employee_id = '{4}'  AND (category_type = 'model' OR category_type = 'Annotation')", "TB_PERFORMANCE_ELEMENT_LOG", startDate, endDate, projectCode, employeeId);
            }

            string dataSource = _configuration.GetConnectionString("DevServerConnectionString");
            return JsonConvert.SerializeObject(transaction.GetTableFromDB(query, dataSource));
        }


    }
}
