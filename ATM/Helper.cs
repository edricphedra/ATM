using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace ATM
{
    public class Helper
    {
        public static SqlDataReader Select(string query)
        {
            SqlConnection sqlConnection;
            string connectionString = @"Data Source=(localdb)\mssqllocaldb;Initial Catalog=DKatalis;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand displayCommand = new SqlCommand(query, sqlConnection);
            SqlDataReader dataReader = displayCommand.ExecuteReader();
            return dataReader;
        }

        public static int InsertUpdate(string query)
        {
            SqlConnection sqlConnection;
            string connectionString = @"Data Source=(localdb)\mssqllocaldb;Initial Catalog=DKatalis;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            SqlCommand insertCommand = new SqlCommand(query, sqlConnection);
            var resultId = insertCommand.ExecuteNonQuery();
            return resultId;
        }
    }
}
