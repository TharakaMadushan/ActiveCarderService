using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace LeaveData
{
    public class Library
    {
        private HttpResponseMessage response;

        public static void WriteErrorLog(string Message)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\ActiveCarderLogFile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + Message);
                sw.Flush();
                sw.Close();
            }
            catch
            {
            }
        }

        public static  void PushActiveCarderData()
        {
            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnFrom"].ConnectionString))
            {
                if (con != null)
                {
                    WriteErrorLog("Connection Started From DBConnFrom");

                    using (var cmd = new SqlCommand("SP_HRM_GetActiveCarderData", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RosterDate", DateTime.Now.Date);
                        con.Open();
                        cmd.ExecuteNonQuery();

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count == 0)
                            {
                                WriteErrorLog("No Data Rows to Import");
                            }
                            else
                            {
                                WriteErrorLog("Selected : " + dt.Rows.Count + " Rows From " + con.Database + " .");

                                using (var client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["JobCostingPostingURL"].ToString());
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    var data = JsonConvert.SerializeObject(dt);
                                    var content = new StringContent(data, Encoding.UTF8, "application/json");
                                     client.PostAsync("api/EmployeeAvailable", content);
                                }
                            }
                        }
                    }
                }
                else
                {
                    WriteErrorLog("Connection didn't Established From DBConnFrom Or Job has been done UnSuccessfully!");
                }
            }
        }
    }
}