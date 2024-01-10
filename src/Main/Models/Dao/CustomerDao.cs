using Main.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Main.Models.Dao
{
    public class CustomerDao
    {
        string connection = "Server=(localdb)\\mssqllocaldb;Database=Main.Data;Trusted_Connection=True;MultipleActiveResultSets=true";

        /// <summary>
        /// 会員登録を行います。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="access_token"></param>
        /// <param name="refresh_token"></param>
        public void InsertCustomer(string userid, string access_token, string refresh_token)
        {
            CommonUtil.Common common = new CommonUtil.Common();
            List<Session> sessions = new List<Session>();

            string sql = string.Empty;
            sql += " INSERT INTO Customer ( ";
            sql += " twitter_id ";
            sql += " , twitter_accesstoken ";
            sql += " , twitter_refreshtoken ";
            sql += " , twitch_accesstoken ";
            sql += " , twitch_refreshtoken ";
            sql += " ) VALUES ( ";
            sql += " " + common.SqlEscape(userid);
            sql += " , " + common.SqlEscape(access_token);
            sql += " , " + common.SqlEscape(refresh_token);
            sql += " , NULL ";
            sql += " , NULL ";
            sql += " ); ";

            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    SqlDataReader result = cmd.ExecuteReader();
                }
            }
        }

        /// <summary>
		/// 会員連携情報取得
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public DataTable SelectCustomer(ref MainContext context)
        {
            CommonUtil.Common common = new CommonUtil.Common();
            List<Session> sessions = new List<Session>();

            string sql = String.Empty;
            sql += " SELECT * ";
            sql += " FROM Customer ";
            sql += " WHERE twitter_id = " + common.SqlEscape(context.Value["twitter_id"].ToString());

            DataTable table = new DataTable();

            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    SqlDataReader result = cmd.ExecuteReader();

                    table.Columns.Add("twitter_id");
                    table.Columns.Add("twitter_accesstoken");
                    table.Columns.Add("twitter_refreshtoken");
                    table.Columns.Add("twitch_accesstoken");
                    table.Columns.Add("twitch_refreshtoken");

                    while (result.Read())
                    {
                        string db_twitter_id = result["twitter_id"].ToString() == null ? string.Empty : result["twitter_id"].ToString();
                        string db_twitteraccesstoken = result["twitter_accesstoken"].ToString() == null ? string.Empty : result["twitter_accesstoken"].ToString();
                        string db_twitter_refreshtoken = result["twitter_refreshtoken"].ToString() == null ? string.Empty : result["twitter_refreshtoken"].ToString();
                        string db_twitch_accesstoken = result["twitch_accesstoken"].ToString() == null ? string.Empty : result["twitch_accesstoken"].ToString();
                        string db_twitch_refreshtoken = result["twitch_refreshtoken"].ToString() == null ? string.Empty : result["twitch_refreshtoken"].ToString();
                        table.Rows.Add(db_twitter_id, db_twitteraccesstoken, db_twitter_refreshtoken, db_twitch_accesstoken, db_twitch_refreshtoken);
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Twitterのアクセストークン、リフレッシュトークンを更新します。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="access_key"></param>
        /// <param name="refresh_key"></param>
        /// <returns></returns>
        public bool UpdateCustomerTwitter(string Id, string access_key, string refresh_key)
        {
            CommonUtil.Common common = new CommonUtil.Common();

            string sql = string.Empty;
            sql += " UPDATE Customer SET ";
            sql += " twitter_accesstoken = " + common.SqlEscape(access_key);
            sql += " , twitter_refreshtoken = " + common.SqlEscape(refresh_key);
            sql += " WHERE id = " + common.SqlEscape(Id);

            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    SqlDataReader result = cmd.ExecuteReader();
                }
            }

            return true;
        }

        /// <summary>
        /// Twitchのアクセストークン、リフレッシュトークンを更新します。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="access_key"></param>
        /// <param name="refresh_key"></param>
        /// <returns></returns>
        public bool UpdateCustomerTwitch(string twitter_id, string access_key, string refresh_key)
        {
            CommonUtil.Common common = new CommonUtil.Common();

            string sql = string.Empty;
            sql += " UPDATE Customer SET ";
            sql += " twitch_accesstoken = " + common.SqlEscape(access_key);
            sql += " , twitch_refreshtoken = " + common.SqlEscape(refresh_key);
            sql += " WHERE twitter_id = " + common.SqlEscape(twitter_id);

            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    SqlDataReader result = cmd.ExecuteReader();
                }
            }

            return true;
        }

        /// <summary>
        /// Twitchのトークンを削除します。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteCustomerTwitch(string id)
        {
            CommonUtil.Common common = new CommonUtil.Common();

            string sql = string.Empty;
            sql += " UPDATE Customer SET ";
            sql += " twitch_accesstoken = NULL ";
            sql += " , twitch_refreshtoken = NULL ";
            sql += " WHERE id = " + common.SqlEscape(id);

            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    SqlDataReader result = cmd.ExecuteReader();
                }
            }

            return true;
        }

        /// <summary>
		/// Twitterのトークンを取得します。
		/// </summary>
		/// <param></param>
		/// <returns></returns>
		public DataTable SelectCustomerTwitterToken()
        {
            CommonUtil.Common common = new CommonUtil.Common();
            List<Session> sessions = new List<Session>();

            string sql = " SELECT * ";
            sql += " FROM Customer ";
            sql += " WHERE twitter_accesstoken IS NOT NULL ";
            sql += " AND twitter_refreshtoken IS NOT NULL ";

            DataTable table = new DataTable();

            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    SqlDataReader result = cmd.ExecuteReader();

                    table.Columns.Add("Id");
                    table.Columns.Add("twitter_accesstoken");
                    table.Columns.Add("twitter_refreshtoken");

                    while (result.Read())
                    {
                        string db_id = result["Id"].ToString() == null ? string.Empty : result["Id"].ToString();
                        string db_twitteraccesstoken = result["twitter_accesstoken"].ToString() == null ? string.Empty : result["twitter_accesstoken"].ToString();
                        string db_twitter_refreshtoken = result["twitter_refreshtoken"].ToString() == null ? string.Empty : result["twitter_refreshtoken"].ToString();
                        table.Rows.Add(db_id, db_twitteraccesstoken, db_twitter_refreshtoken);
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Twitchのトークンを取得します。
        /// </summary>
        /// <returns></returns>
        public DataTable SelectCustomerTwitchToken()
        {
            CommonUtil.Common common = new CommonUtil.Common();
            List<Session> sessions = new List<Session>();

            string sql = " SELECT * ";
            sql += " FROM Customer ";
            sql += " WHERE twitch_accesstoken IS NOT NULL ";
            sql += " AND twitch_refreshtoken IS NOT NULL ";

            DataTable table = new DataTable();

            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    SqlDataReader result = cmd.ExecuteReader();

                    table.Columns.Add("Id");
                    table.Columns.Add("twitch_accesstoken");
                    table.Columns.Add("twitch_refreshtoken");

                    while (result.Read())
                    {
                        string db_id = result["Id"].ToString() == null ? string.Empty : result["Id"].ToString();
                        string db_twitch_accesstoken = result["twitch_accesstoken"].ToString() == null ? string.Empty : result["twitch_accesstoken"].ToString();
                        string db_twitch_refreshtoken = result["twitch_refreshtoken"].ToString() == null ? string.Empty : result["twitch_refreshtoken"].ToString();
                        table.Rows.Add(db_id, db_twitch_accesstoken, db_twitch_refreshtoken);
                    }
                }
            }

            return table;
        }
    }
}
