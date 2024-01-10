using Main.Data;
using Main.Models.Dao;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Text;
using System.Security.Cryptography;
using Main.Api.Twitch;
using Main.Api.Twitter.Dto;
using Main.Api.Twitch.Dto;
using Main.Api.StreamerLiving.Dto;
using Newtonsoft.Json;
using System;

namespace Main.Controllers
{
    public class ApiController : Controller
    {
        MainContext context;
        string salt = "xxx"; // ハッシュソルト 64文字くらい

        /// <summary>
        /// Twitter情報取得用画面
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<SL_ResponseTwitterDto> Twitter([FromBody] SL_RequestTwitterDto reqdata)
        {
            Init(ref context);
            if (!RequiredLoginPage(ref context))
            {
                return null;
            }

            if (reqdata == null)
            {
                return null;
            }

            string[] twitter_id;
            if (reqdata.twitter_id == null)
            {
                return null;
            }
            else
            {
                twitter_id = reqdata.twitter_id;
            }

            string[] tweet_id = reqdata.tweet_id;

            List<SL_ResponseTwitterDto> response = GetStreamerTwitter(twitter_id, tweet_id);

            return response;
        }

        /// <summary>
        /// Live情報取得用画面
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<SL_ResponseLiveDto> Live([FromBody] SL_RequestLivedto reqdata)
        {
            Init(ref context);
            if (!RequiredLoginPage(ref context))
            {
                return null;
            }

            if (reqdata == null)
            {
                return null;
            }

            string[] youtube;
            if (reqdata.youtube == null)
            {
                youtube = null;
            }
            else
            {
                youtube = reqdata.youtube;
            }

            string[] twitch;
            if (reqdata.twitch == null)
            {
                twitch = null;
            }
            else
            {
                twitch = reqdata.twitch;
            }

            List<Twitch_data> twitch_data = GetLiveStreamerInTwitch(twitch);
            List<Youtube_data> youtube_data = GetLiveStreamerInYoutube(youtube);

            List<SL_ResponseLiveDto> response = GetLive(twitch_data, youtube_data);

            return response;
        }

        /// <summary>
        /// 配信者情報登録画面
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        [HttpPost]
        public string RegistStreamer([FromBody] SL_RequestRegistStreamerDto reqdata)
        {
            // ログイン情報確認
            Init(ref context);
            if (!RequiredLoginPage(ref context))
            {
                return JsonConvert.SerializeObject(new SL_ResponseStreamerDto { result = false, msg = "不正なデータです。" });
            }

            string errormsg = ValidateRegistStreamer(reqdata);
            if (!string.IsNullOrEmpty(errormsg))
            {
                return JsonConvert.SerializeObject(new SL_ResponseStreamerDto { result = false, msg = errormsg });
            }

            // 登録情報取得
            StreamerDao dao = new StreamerDao();
            DataTable table = dao.SelectStreamer(context);
            bool[] regist_flg = new bool[] { false, false, false, false, false };
            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    regist_flg[int.Parse(row["num"].ToString()) - 1] = true;
                }
            }

            // 既に登録されているデータの場合は更新、そうでない場合は登録
            if (regist_flg[(int)reqdata.num - 1] == true)
            {
                dao.UpdateStreamer(context, reqdata.num.ToString(), reqdata.name, reqdata.twitter, reqdata.youtube, reqdata.twitch);
            }
            else
            {
                dao.InsertStreamer(context, reqdata.num.ToString(), reqdata.name, reqdata.twitter, reqdata.youtube, reqdata.twitch);
            }

            return JsonConvert.SerializeObject(new SL_ResponseStreamerDto { result = true, msg = string.Empty });
        }

        /// <summary>
        /// 配信者情報削除画面
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        [HttpPost]
        public string DeleteStreamer([FromBody] SL_RequestDeleteStreamerDto reqdata)
        {
            // ログイン情報確認
            Init(ref context);
            if (!RequiredLoginPage(ref context))
            {
                return JsonConvert.SerializeObject(new SL_ResponseStreamerDto { result = false, msg = "不正なデータです。" });
            }

            string errormsg = ValidateDeleteStreamer(reqdata);
            if (!string.IsNullOrEmpty(errormsg))
            {
                return JsonConvert.SerializeObject(new SL_ResponseStreamerDto { result = false, msg = errormsg });
            }

            // 登録情報取得
            StreamerDao dao = new StreamerDao();
            DataTable table = dao.SelectStreamer(context);
            bool[] regist_flg = new bool[] { false, false, false, false, false };
            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    regist_flg[int.Parse(row["num"].ToString()) - 1] = true;
                }
            }
            if (regist_flg[(int)reqdata.num - 1] == true)
            {
                dao.DeleteStreamer(context, reqdata.num.ToString());
                return JsonConvert.SerializeObject(new SL_ResponseStreamerDto { result = true, msg = string.Empty });
            }
            else
            {
                return JsonConvert.SerializeObject(new SL_ResponseStreamerDto { result = false, msg = "該当データがありません。" });
            }
        }





        public class Youtube_data
        {
            public int num { get; set; }
            public string title { get; set; }
            public string thumbnail_url { get; set; }
        }

        /// <summary>
        /// Twitch
        /// </summary>
        public class Twitch_data
        {
            public int num { get; set; }
            public string title { get; set; }
            public string thumbnail_url { get; set; }
        }

        /// <summary>
        /// 配信情報
        /// </summary>
        public class Live_data
        {
            public bool is_live_youtube { get; set; }
            public bool is_live_twitch { get; set; }
        }

        /// <summary>
        /// 配信者のツイート情報を取得します。
        /// </summary>
        /// <param name="streamer_id">TwitterID一覧(例：[884207361400815616, "", "", "", ""])</param>
        /// <param name="tweet_id">最新ツイートID一覧(例：[1614530248108027905, null, null, null, null])</param>
        /// <param name="num">ツイート取得件数</param>
        /// <returns></returns>
        public List<SL_ResponseTwitterDto> GetStreamerTwitter(string[]? streamer_id, string[]? tweet_id)
        {
            string[] tweet_list = new string[5];
            for (int tw_i = 0; tw_i < tweet_list.Length; tw_i++)
            {
                tweet_list[tw_i] = string.Empty;
            }

            // 取得するための情報がないためスキップ
            if (streamer_id == null) { return null; }

            List<SL_ResponseTwitterDto> twitter_datas = new List<SL_ResponseTwitterDto>();

            int streamer_i = -1;
            // 配信者ごとのループ
            foreach (string streamer in streamer_id)
            {
                streamer_i += 1;
                // 空の時は取得する必要がないためスキップ
                if (string.IsNullOrEmpty(streamer)) { continue; }

                TwitterApi twitterapi = new TwitterApi();
                string id = twitterapi.RequestUserIdFromUserName(context.Value["twitter_accesstoken"].ToString(), streamer);

                // id が取得できない時はツイート情報が取得できないためスキップ
                if (string.IsNullOrEmpty(id)) { continue; }

                int num = 1;
                if (tweet_id != null && !string.IsNullOrEmpty(tweet_id[streamer_i]))
                {
                    num = 5;
                }
                else
                {
                    num = 10;
                }

                Twitter_TweetsDto tweets = twitterapi.RequestStreamerTweets(context.Value["twitter_accesstoken"].ToString(), id, num);

                // ツイート情報が取得できない時はスキップ
                if (tweets == null || tweets.data == null) { continue; }

                Twitter_TweetsDto_data[] data = tweets.data;
                Twitter_TweetsDto_includes includes = tweets.includes;

                List<string> res_id = new List<string>();
                List<string> res_usericon = new List<string>();
                List<string> res_username = new List<string>();
                List<string> res_userid = new List<string>();
                List<string> res_created_at = new List<string>();
                List<string> res_text = new List<string>();

                // ツイートごとのループ
                for (int i = 0; i < data.Length; i++)
                {
                    // 取得したツイートに表示済みのツイートがある場合、それ以降のツイートは追加する必要がないためスキップ
                    if (tweet_id != null && !string.IsNullOrEmpty(tweet_id[streamer_i]) && data[i].id == tweet_id[streamer_i])
                    {
                        break;
                    }

                    // リプライのツイートでリプライ先が本人以外の場合、表示対象外
                    if (data[i].in_reply_to_user_id != null && data[i].in_reply_to_user_id != id)
                    {
                        continue;
                    }

                    res_id.Add(data[i].id);
                    foreach (Twitter_TweetsDto_users user in includes.users)
                    {
                        // ツイートしたユーザー情報
                        if (user.id == data[i].author_id)
                        {
                            res_usericon.Add(user.profile_image_url);
                            res_username.Add(user.name);
                            res_userid.Add(user.username);
                            break;
                        }
                    }
                    res_created_at.Add(DateTime.Parse(data[i].created_at, null, System.Globalization.DateTimeStyles.RoundtripKind).AddHours(9).ToString("MM/dd HH:mm"));
                    res_text.Add(data[i].text);
                }

                twitter_datas.Add(new SL_ResponseTwitterDto
                {
                    num = streamer_i,
                    id  = res_id,
                    user_icon = res_usericon,
                    user_name = res_username,
                    user_id = res_userid,
                    created_at = res_created_at,
                    tweet = res_text
                });
            }

            return twitter_datas;
        }

        /// <summary>
        /// Twitch 配信情報を取得します。
        /// </summary>
        /// <param name="streamer_id"></param>
        /// <returns></returns>
        public List<Twitch_data> GetLiveStreamerInTwitch(string[]? streamer_id)
        {
            // 取得するための情報がないためスキップ
            if (streamer_id == null) { return null; }

            List<Twitch_data> twitch_data = new List<Twitch_data>();

            int streamer_i = -1;
            // 配信者ごとのループ
            foreach (string streamer in streamer_id)
            {
                streamer_i += 1;
                // 空の時は取得する必要がないためスキップ
                if (string.IsNullOrEmpty(streamer)) { continue; }

                TwitchApi twitchapi = new TwitchApi();
                Twitch_HelixStreamDto livedata = twitchapi.RequestHelixStreams(context, streamer);

                // 配信情報が取得できない時はスキップ
                if (livedata == null || livedata.data == null || livedata.data.Length == 0) { continue; }

                Twitch_HelixStream_data[] data = livedata.data;

                string title = string.Empty;
                if (data != null && data.Length >= streamer_i && data[streamer_i - 1].title != null)
                {
                    title = data[streamer_i - 1].title;
                }
                string thumbnail_url = string.Empty;
                if (data != null && data.Length >= streamer_i && data[streamer_i - 1].thumbnail_url != null)
                {
                    thumbnail_url = data[streamer_i - 1].thumbnail_url;
                }

                twitch_data.Add(new Twitch_data
                {
                    num = streamer_i,
                    title = title,
                    thumbnail_url = thumbnail_url
                });
            }

            return twitch_data;
        }

        /// <summary>
        /// Youtube 配信情報を取得します。
        /// </summary>
        /// <param name="streamer_id"></param>
        /// <returns></returns>
        public List<Youtube_data> GetLiveStreamerInYoutube(string[]? streamer_id)
        {
            // 取得するための情報がないためスキップ
            if (streamer_id == null) { return null; }

            List<Youtube_data> youtube_data = new List<Youtube_data>();

            int streamer_i = -1;
            // 配信者ごとのループ
            foreach (string streamer in streamer_id)
            {
                streamer_i += 1;
                // 空の時は取得する必要がないためスキップ
                if (string.IsNullOrEmpty(streamer)) { continue; }

            }

            return youtube_data;
        }

        private List<SL_ResponseLiveDto> GetLive(List<Twitch_data> twitch_data, List<Youtube_data> youtube_data)
        {
            List<SL_ResponseLiveDto> response = new List<SL_ResponseLiveDto>();

            if (twitch_data != null)
            {
                foreach (Twitch_data item in twitch_data)
                {
                    response.Add(new SL_ResponseLiveDto {
                        youtube = false,
                        twitch = true
                    });
                }
            }

            return response;
        }

        /// <summary>
        /// 登録情報の入力チェック
        /// </summary>
        /// <param name="resdata"></param>
        /// <returns></returns>
        private string ValidateRegistStreamer(SL_RequestRegistStreamerDto? resdata)
        {
            if (resdata == null) { return "不正なデータです。"; }

            if (resdata.num == null || resdata.num < 1 || resdata.num > 5)
            {
                return "値が不正です。";
            }

            if (string.IsNullOrEmpty(resdata.name))
            {
                return "表示名が入力されていません。";
            }

            if (resdata.name.Length > 256)
            {
                return "表示名が入力されていません。";
            }

            if (!string.IsNullOrEmpty(resdata.twitter) && resdata.twitter.Length > 256)
            {
                return "TwitterIDが正しくありません。";
            }

            if (!string.IsNullOrEmpty(resdata.youtube) && resdata.youtube.Length > 256)
            {
                return "Youtubeが正しくありません。";
            }

            if (!string.IsNullOrEmpty(resdata.twitch) && resdata.twitch.Length > 256)
            {
                return "Twitchが正しくありません。";
            }

            return string.Empty;
        }

        /// <summary>
        /// 削除情報の入力チェック
        /// </summary>
        /// <param name="resdata"></param>
        /// <returns></returns>
        private string ValidateDeleteStreamer(SL_RequestDeleteStreamerDto? resdata)
        {
            if (resdata == null) { return "不正なデータです。"; }

            // 入力チェック
            if (resdata.num == null || resdata.num < 1 || resdata.num > 5)
            {
                return "値が不正です。";
            }

            return string.Empty;
        }





        #region private

        /// <summary>
        /// 共通処理_開始時処理
        /// </summary>
        private void Init(ref MainContext context)
        {
            CommonController common = new CommonController(ref context, Request);
            CheckCookieSession(ref context);
        }

        /// <summary>
        /// Cookie確認処理
        /// </summary>
        private void CheckCookieSession(ref MainContext context)
        {
            int id;
            int session = -1;
            SessionDao dao = new SessionDao();

            context.Value["session"] = session;
            context.Value["loginsession"] = string.Empty;
            context.Value["twitter_id"] = string.Empty;
            context.Value["twitch_state"] = string.Empty;

            // Cookieのセッション番号が空、もしくは数値に変換できない場合
            if (string.IsNullOrEmpty(Request.Cookies["sl_session_id"]) || !int.TryParse(Request.Cookies["sl_session_id"], out id))
            {
                session = dao.CreateSession(context);
                SetCookie(session);
                context.Value["session"] = session;
                context.Value["twitter_id"] = string.Empty;
                return;
            }
            // Cookieのセッション番号が数値であれば代入
            else
            {
                session = id;
            }

            DataTable table = dao.SelectSession(session);
            string loginsession = string.Empty;
            // Sessionの値が存在しなければSession作成
            if (table.Rows.Count <= 0)
            {
                session = dao.CreateSession(context);
                SetCookie(session);
                context.Value["session"] = session;
                context.Value["twitter_id"] = string.Empty;
                return;
            }
            else
            {
                foreach (DataRow row in table.Rows)
                {
                    context.Value["twitter_id"] = row["twitter_id"].ToString();
                    loginsession = row["loginsession"].ToString();
                    context.Value["twitter_state"] = row["twitter_state"].ToString();
                    context.Value["twitter_code_challenge"] = row["twitter_code_challenge"].ToString();
                    context.Value["twitch_state"] = row["twitch_state"].ToString();
                    context.Value["sessoin_dt"] = DateTime.Parse(row["dt"].ToString());
                    context.Value["now"] = DateTime.Parse(row["now"].ToString());
                }
            }

            // ログインしているSessionの場合は値が正しいかチェック
            if (!string.IsNullOrEmpty(context.Value["twitter_id"].ToString()))
            {
                SHA512CryptoServiceProvider sha = new SHA512CryptoServiceProvider();
                string saltandlogin = salt + loginsession;
                byte[] data = Encoding.ASCII.GetBytes(saltandlogin);
                // 正しいセッション
                if (Convert.ToBase64String(sha.ComputeHash(data)) == Request.Cookies["sl_login_session"])
                {
                    // セッション更新日が現在日時から7日より前である場合はセッションを新規作成
                    if (DateTime.Parse(context.Value["sessoin_dt"].ToString()).AddDays(7) < DateTime.Parse(context.Value["now"].ToString()))
                    {
                        session = dao.CreateSession(context);
                        SetCookie(session);
                        context.Value["session"] = session;
                        context.Value["twitter_id"] = string.Empty;
                        context.Value["twitter_state"] = string.Empty;
                        context.Value["twitter_code_challenge"] = string.Empty;
                        context.Value["twitch_state"] = string.Empty;
                        return;
                    }
                    else
                    {
                        context.Value["session"] = session;
                        context.Value["loginsession"] = Request.Cookies["sl_login_session"];
                        dao.UpdateSessionDt(context);
                        return;
                    }
                }
                else
                {
                    session = dao.CreateSession(context);
                    SetCookie(session);
                    context.Value["session"] = session;
                    context.Value["twitter_id"] = string.Empty;
                    context.Value["twitter_state"] = string.Empty;
                    context.Value["twitter_code_challenge"] = string.Empty;
                    context.Value["twitch_state"] = string.Empty;
                    return;
                }
            }

            // ログインしていない正しいSession
            context.Value["session"] = session;
            context.Value["loginsession"] = string.Empty;
            context.Value["twitter_id"] = string.Empty;
            context.Value["twitch_state"] = string.Empty;
            return;
        }

        /// <summary>
        /// Cookie作成
        /// </summary>
        private void SetCookie(int session)
        {
            // Cookieに設定.
            Response.Cookies.Append("sl_session_id", session.ToString());
            Response.Cookies.Append("sl_login_session", string.Empty);
        }

        private bool RequiredLoginPage(ref MainContext context)
        {
            SelectAlignment(ref context);

            if (string.IsNullOrEmpty(context.Value["twitter_id"].ToString()))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 会員情報を取得してcontextに設定します。
        /// </summary>
        /// <param name="context"></param>
        private void SelectAlignment(ref MainContext context)
        {
            context.Value["twitter_accesstoken"] = string.Empty;
            context.Value["twitter_refreshtoken"] = string.Empty;
            context.Value["twitch_accesstoken"] = string.Empty;
            context.Value["twitch_refreshtoken"] = string.Empty;

            if (string.IsNullOrEmpty(context.Value["twitter_id"].ToString()))
            {
                return;
            }

            CustomerDao dao = new CustomerDao();
            DataTable table = dao.SelectCustomer(ref context);

            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    context.Value["twitter_accesstoken"] = row["twitter_accesstoken"].ToString();
                    context.Value["twitter_refreshtoken"] = row["twitter_refreshtoken"].ToString();
                    context.Value["twitch_accesstoken"] = row["twitch_accesstoken"].ToString();
                    context.Value["twitch_refreshtoken"] = row["twitch_refreshtoken"].ToString();
                }
            }
        }

        #endregion private



    }
}
