using Main.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace Main.Models.Dao
{
    public class StreamerDao
    {
        string connection = "Server=(localdb)\\mssqllocaldb;Database=Main.Data;Trusted_Connection=True;MultipleActiveResultSets=true";

        /// <summary>
        /// 配信者情報を取得します。
        /// </summary>
        /// <param name="context"></param>
        /// <returns>登録配信者情報一覧</returns>
        public DataTable SelectStreamer(MainContext context)
        {
            CommonUtil.Common common = new CommonUtil.Common();
            List<Session> sessions = new List<Session>();

            string sql = String.Empty;
            sql += " SELECT * ";
            sql += " FROM Streamer ";
            sql += " WHERE twitter_id = " + common.SqlEscape(context.Value["twitter_id"].ToString());

            DataTable table = new DataTable();

            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    SqlDataReader result = cmd.ExecuteReader();

                    table.Columns.Add("num");
                    table.Columns.Add("name");
                    table.Columns.Add("twitter");
                    table.Columns.Add("youtube");
                    table.Columns.Add("twitch");

                    while (result.Read())
                    {
                        string db_num = result["num"].ToString() == null ? string.Empty : result["num"].ToString();
                        string db_name = result["name"].ToString() == null ? string.Empty : result["name"].ToString();
                        string db_twitter = result["twitter"].ToString() == null ? string.Empty : result["twitter"].ToString();
                        string db_youtube = result["youtube"].ToString() == null ? string.Empty : result["youtube"].ToString();
                        string db_twitch = result["twitch"].ToString() == null ? string.Empty : result["twitch"].ToString();
                        table.Rows.Add(db_num, db_name, db_twitter, db_youtube, db_twitch);
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// 配信者情報を登録します。
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="access_token"></param>
        /// <param name="refresh_token"></param>
        public void InsertStreamer(MainContext context, string num, string name, string? twitter, string? youtube, string? twitch)
        {
            CommonUtil.Common common = new CommonUtil.Common();
            List<Session> sessions = new List<Session>();

            string sql = string.Empty;
            sql += " INSERT INTO Streamer ( ";
            sql += " twitter_id ";
            sql += " , num ";
            sql += " , name ";
            sql += " , twitter ";
            sql += " , youtube ";
            sql += " , twitch ";
            sql += " ) VALUES ( ";
            sql += " " + common.SqlEscape(context.Value["twitter_id"].ToString());
            sql += " , " + common.SqlEscape(num);
            sql += " , " + common.SqlEscape(name);
            sql += " , " + common.SqlEscape(twitter);
            sql += " , " + common.SqlEscape(youtube);
            sql += " , " + common.SqlEscape(twitch);
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
        /// 配信者情報を更新します。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="access_key"></param>
        /// <param name="refresh_key"></param>
        public void UpdateStreamer(MainContext context, string num, string name, string? twitter, string? youtube, string? twitch)
        {
            CommonUtil.Common common = new CommonUtil.Common();

            string sql = string.Empty;
            sql += " UPDATE Streamer SET ";
            sql += " name = " + common.SqlEscape(name);
            sql += " , twitter = " + common.SqlEscape(twitter);
            sql += " , youtube = " + common.SqlEscape(youtube);
            sql += " , twitch = " + common.SqlEscape(twitch);
            sql += " WHERE twitter_id = " + common.SqlEscape(context.Value["twitter_id"].ToString());
            sql += " AND num = " + common.SqlEscape(num);

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
        /// 配信者情報を削除します。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="num"></param>
        public void DeleteStreamer(MainContext context, string num)
        {
            CommonUtil.Common common = new CommonUtil.Common();

            string sql = string.Empty;
            sql += " DELETE FROM Streamer ";
            sql += " WHERE twitter_id = " + common.SqlEscape(context.Value["twitter_id"].ToString());
            sql += " AND num = " + common.SqlEscape(num);

            using (SqlConnection con = new SqlConnection(connection))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    SqlDataReader result = cmd.ExecuteReader();
                }
            }
        }
    }
}
