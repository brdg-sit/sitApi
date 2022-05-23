using System.Data;
using System.Data.SqlClient;

namespace UnrealViewerAPI.Controllers
{
    public class Transaction
    {
        public DataTable GetTableFromDB(string query, string dataSource)
        {
            DataTable table = new DataTable();
            SqlDataReader sqlDataReader;

            using (SqlConnection sqlConnection = new SqlConnection(dataSource))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlCommand.CommandTimeout = 60;
                    sqlDataReader = sqlCommand.ExecuteReader();
                    table.Load(sqlDataReader);
                    sqlConnection.Close();
                }
            }

            return table;
        }
        public Task<DataSet> GetDataSetAsync(string sConnectionString, string sSQL, int cur, int page)
        {
            return Task.Run(() =>
            {
                using (var newConnection = new SqlConnection(sConnectionString))
                using (var mySQLAdapter = new SqlDataAdapter(sSQL, newConnection))
                {
                    mySQLAdapter.SelectCommand.CommandType = CommandType.Text;

                    DataSet myDataSet = new DataSet();
                    mySQLAdapter.Fill(myDataSet, cur, page, "TABLE");
                    return myDataSet;
                }
            });
        }
        public DataSet GetPagingData(string query, string dataSource, string table, int currentIndex = 0, int pageSize = 5)
        {
            DataSet dataSet = new DataSet();

            //int currentIndex = 0;
            //int pageSize = 5;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(dataSource))
                {
                    sqlConnection.Open();

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, sqlConnection);

                    sqlDataAdapter.Fill(dataSet, currentIndex, pageSize, table);

                    sqlConnection.Close();
                }
            }
            catch (Exception)
            {
            }

            return dataSet;
        }

        public void ExecuteNonQuery(string query, string dataSource)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(dataSource))
                {
                    sqlConnection.Open();
                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
