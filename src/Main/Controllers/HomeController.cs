using Azure.Core;
using Main.Api.Twitch;
using Main.Api.Twitter.Dto;
using Main.Data;
using Main.Models.Dao;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace Main.Controllers
{
    public class HomeController : Controller
    {
        MainContext context;
		string salt = "xxxxx"; // ハッシュソルト 64文字くらい

        // 初期処理
        public HomeController(ILogger<HomeController> logger)
        {
			SeedingAsync(context);
            Task<string> twitter = ValidateTwitterToken();
            Task<string> twitch = ValidateTwitchToken();
        }

        /// <summary>
        /// メイン画面の表示処理を行います。
        /// </summary>
        /// <remarks>
        /// ログイン必須
        /// </remarks>
        /// <returns>画面の表示内容</returns>
        public IActionResult Index()
        {
            Init(ref context);
            if (!RequiredLoginPage(ref context))
            {
                return View("Login");
            }

            // 表示情報取得
            GetStreamer();

            ViewData["session"] = context.Value["session"].ToString();
            ViewData["loginsession"] = context.Value["loginsession"].ToString();

            return View();
        }

        /// <summary>
        /// 登録情報一覧画面の表示処理を行います。
        /// </summary>
        /// <param name="name">表示名</param>
        /// <param name="twitter">TwitterID(例：say_matane)</param>
        /// <param name="youtube">YoutubeURL(例：)</param>
        /// <param name="twitch">TwitchURL(例：)</param>
        /// <remarks>
        /// ログイン必須
        /// </remarks>
        /// <returns>画面の表示内容</returns>
        [AutoValidateAntiforgeryToken]
        // リクエスト内容がGETだと外部から削除可能になってしまうので、POSTパラメータだけ取るようにする
        public IActionResult List()
        {
            Init(ref context);
            if (!RequiredLoginPage(ref context))
            {
                return View("Login");
            }

            // パラメータの内容に応じて取り消し処理を実施する
            TwitchApi twitchapi = new TwitchApi();

            // Twitter トークン取り消し処理
            if (Request.Query["twitter_revoke"] == "true")
            {
                TwitterApi twitterapi = new TwitterApi();
                twitterapi.RequestRevokeAccessToken(context);
                SetCookie(int.Parse(context.Value["session"].ToString()));
                Response.Redirect(CommonUtil.Common.BaseUrl + "/Login");
            }

            // Twitch トークン取り消し処理
            if (Request.Query["twitch_revoke"] == "true")
            {
                twitchapi.RequestRevokeAccessToken(context);
            }

            // Twitchの連携状況に応じて表示内容の出し分け
            if (string.IsNullOrEmpty(context.Value["twitch_accesstoken"].ToString()))
            {
                ViewData["twitch_connection"] = false;
                ViewData["redirect_url"] = twitchapi.CreateAuthorizationCodeUrl(context);
            }
            else
            {
                ViewData["twitch_connection"] = true;
            }

            ViewData["errormsg"] = string.Empty;

            // 配信者情報の登録更新削除処理、表示処理
            GetStreamer();

            return View();
        }

        /// <summary>
		/// Twitter callback画面
		/// </summary>
		/// <remarks>
		/// ログイン前に通る画面のため、ログイン不要
		/// </remarks>
		/// <returns></returns>
        public IActionResult Twitter_GetAccessToken()
        {
            Init(ref context);

            // リクエスト値設定
            Api.Twitter.Dto.AuthorizationResponseDto dto = new Api.Twitter.Dto.AuthorizationResponseDto();
            dto.state = Request.Query["state"];
            dto.code = Request.Query["code"];

            // CSRF対策でパラメータのstateとリクエスト時のstateを比較する
            // https://developer.twitter.com/en/docs/authentication/oauth-2-0/authorization-code
            if (dto.state != context.Value["twitter_state"].ToString())
            {
                Response.Redirect(CommonUtil.Common.BaseUrl + "/?twitter_error1=true");
                return View();
            }

            TwitterApi twitterapi = new TwitterApi();
            twitterapi.RequestAccessToken(ref context, dto.code);
            if (!string.IsNullOrEmpty(context.Value["twitter_id"].ToString()) && !string.IsNullOrEmpty(context.Value["loginsession"].ToString()))
            {
                // Cookieに反映
                SHA512CryptoServiceProvider sha = new SHA512CryptoServiceProvider();
                string saltandlogin = salt + context.Value["loginsession"].ToString();
                byte[] data = Encoding.ASCII.GetBytes(saltandlogin);
                Response.Cookies.Append("sl_login_session", Convert.ToBase64String(sha.ComputeHash(data)));
                // トップ画面に遷移
                Response.Redirect(CommonUtil.Common.BaseUrl);
            }
            else
            {
                // 異常時、ログイン画面へ遷移
                Response.Redirect(CommonUtil.Common.BaseUrl + "/?twitter_error2=true");
            }

            return View();
        }

        /// <summary>
        /// Twitch認証用画面
        /// </summary>
        /// <remarks>
        /// ログイン必須
        /// </remarks>
        /// <returns></returns>
        public IActionResult Twitch_GetAccessToken()
        {
            Init(ref context);
            if (!RequiredLoginPage(ref context))
            {
                return View("Login");
            }

            // リクエスト値設定
            Api.Twitch.Dto.Twitch_AuthorizationResponseDto dto = new Api.Twitch.Dto.Twitch_AuthorizationResponseDto();
            dto.code = Request.Query["code"];
            dto.scope = Request.Query["scope"];
            dto.state = Request.Query["state"];
            dto.error = Request.Query["error"];
            dto.error_description = Request.Query["error_description"];

            // リクエストが異常値の場合、登録情報一覧画面へ遷移
            if (string.IsNullOrEmpty(dto.code) || string.IsNullOrEmpty(dto.state) || !string.IsNullOrEmpty(dto.error) || !string.IsNullOrEmpty(dto.error_description))
            {
                Response.Redirect(CommonUtil.Common.BaseUrl + "/List?twitch_error=true");
                return View();
            }

            SessionDao dao = new SessionDao();
            DataTable table = dao.SelectSession(int.Parse(context.Value["session"].ToString()));
            string state = string.Empty;
            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    state = row["twitch_state"].ToString();
                }
            }

            // CSRF対策でパラメータのstateとリクエスト時のstateを比較する
            if (dto.state != "" && dto.state != state)
            {
                Response.Redirect(CommonUtil.Common.BaseUrl + "/List?twitch_error=true");
                return View();
            }

            TwitchApi twitchapi = new TwitchApi();
            if (twitchapi.RequestGetAccessToken(context, dto.code))
            {
                // 成功時、トップ画面へ遷移
                Response.Redirect(CommonUtil.Common.BaseUrl);
            }
            else
            {
                // 失敗時登録情報一覧画面へ遷移
                Response.Redirect(CommonUtil.Common.BaseUrl + "/List?twitch_error=true");
            }

            return View();
        }





        #region 共通処理

        /// <summary>
        /// DB 初期データ投入
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<string> SeedingAsync(MainContext context)
        {
            var optionsBuilder = new DbContextOptions<MainContext>();
            context = new MainContext(optionsBuilder);
            await context.Database.EnsureCreatedAsync(); // DBが存在しない場合はDB作成
            await context.SaveChangesAsync();
            return "";
        }

        /// <summary>
        /// ログイン必須画面の処理を行います。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool RequiredLoginPage(ref MainContext context)
        {
            SelectAlignment(ref context);

            if (string.IsNullOrEmpty(context.Value["twitter_id"].ToString()))
            {
                // ログインボタンのURL
                TwitterApi twitterapi = new TwitterApi();
                ViewData["twitter_authorization_url"] = twitterapi.CreateAuthorizationUrl(context);

                return false;
            }

            return true;
		}

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

        /// <summary>
        /// 配信者情報を取得します。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="name"></param>
        /// <param name="twitter"></param>
        /// <param name="youtube"></param>
        /// <param name="twitch"></param>
        private void GetStreamer()
        {
            // 登録情報取得
            StreamerDao dao = new StreamerDao();
            DataTable table = dao.SelectStreamer(context);

            // 表示内容
            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ViewData["name_" + row["num"]] = row["name"].ToString();
                    ViewData["twitter_" + row["num"]] = row["twitter"].ToString();
                    ViewData["youtube_" + row["num"]] = row["youtube"].ToString();
                    ViewData["twitch_" + row["num"]] = row["twitch"].ToString();
                }
            }
        }





        /// <summary>
        /// Twitterのアクセストークンを更新します。
        /// </summary>
        /// <returns></returns>
        private static async Task<string> ValidateTwitterToken()
        {
            TwitterApi twitterapi = new TwitterApi();
            while (true)
            {
                CustomerDao dao = new CustomerDao();
                DataTable table = dao.SelectCustomerTwitterToken();
                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        twitterapi.RequestUpdateAccessToken(row["Id"].ToString(), row["twitter_refreshtoken"].ToString());
                    }
                }
                await Task.Delay(1000 * 60 * 60); // 60分
            }
        }

        /// <summary>
        /// Twitchのアクセストークンを検証します。トークンが有効でない場合、トークンを更新します。
        /// </summary>
        /// <remarks>
        /// API: https://dev.twitch.tv/docs/authentication/validate-tokens
        /// 1時間ごとに検証する必要がある
        /// </remarks>
        /// <returns></returns>
        private static async Task<string> ValidateTwitchToken()
        {
            TwitchApi twitchapi = new TwitchApi();
            while (true)
            {
                CustomerDao dao = new CustomerDao();
                DataTable table = dao.SelectCustomerTwitchToken();
                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        twitchapi.RequestValidateAccessToken(row["twitter_id"].ToString(), row["twitch_accesstoken"].ToString(), row["twitch_refreshtoken"].ToString());
                    }
                }
                await Task.Delay(1000 * 60 * 30); // 30分
            }
        }

        #endregion
    }
}