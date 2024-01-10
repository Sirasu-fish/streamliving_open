using Main.Data;
using Main.Models.Dao;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Web;
using Main.Api.Twitch.Dto;
using Main.Api.Twitter.Dto;
using Azure.Core;

namespace Main.Api.Twitch
{
	public class TwitchApi
	{
		string clientID = "xxxxx"; // クライアントID 多分30文字
		string secretToken = "xxxxx"; // シークレットトークン 多分30文字
		string redirect_uri = "https://localhost/main/home/Twitch_GetAccessToken/"; //https://dev.twitch.tv/console/apps

		/// <summary>
		/// アプリ承認フローURL生成
		/// </summary>
		/// <remarks>https://dev.twitch.tv/docs/authentication/getting-tokens-oauth#authorization-code-grant-flow</remarks>
		/// <returns></returns>
		public string CreateAuthorizationCodeUrl(MainContext context)
		{
			string url = "https://id.twitch.tv/oauth2/authorize";
			string param = string.Empty;
			string state = RandomString(32);

			param += "?response_type=code";
			param += "&client_id=" + clientID;
			param += "&redirect_uri=" + HttpUtility.UrlEncode(redirect_uri);
			// https://dev.twitch.tv/docs/authentication/scopes
			param += "&scope=";
			param += "&state=" + state;

			Models.Dao.SessionDao dao = new Models.Dao.SessionDao();
			bool result = dao.UpdateSessionForTwitch(context, state);

			url = url + param;

			return url;
		}

		/// <summary>
		/// 認証コード付与フローのリクエストを行います
		/// </summary>
		/// <param name="code"></param>
		/// <remarks>
		/// API: https://dev.twitch.tv/docs/authentication/getting-tokens-oauth#authorization-code-grant-flow
		/// curl: curl -request POST -d 'Content-Type: application/x-www-form-urlencoded' -d 'Content-Length: 1000' -d "client_id=wahx7ogta4k72inicil8t8po15uv4u&client_secret=6tsgfxual5feot5feowwn8o28nd9g5&code=x03g1j6i4jpqfcxgtf8y648y7dni27&grant_type=authorization_code&redirect_uri=https://localhost/main/home/twitch_redirect_uri/" "https://id.twitch.tv/oauth2/token"
		/// </remarks>
		/// <returns></returns>
		public bool RequestGetAccessToken(MainContext context, string code)
		{
			// リクエスト内容定義
			string url = "https://id.twitch.tv/oauth2/token";
			string param = string.Empty;
			param += "client_id=" + clientID;
			param += "&client_secret=" + secretToken;
			param += "&code=" + code;
			param += "&grant_type=authorization_code";
			param += "&redirect_uri=" + redirect_uri;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
			req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
			byte[] data = Encoding.ASCII.GetBytes(param);

            req.ContentLength = data.Length;

            req.Timeout = 30 * 1000;

            string resBodyStr = string.Empty;
			HttpStatusCode resStatusCode = HttpStatusCode.NotFound;

            Stream reqStream = req.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            try
			{
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                Stream st = res.GetResponseStream();
                StreamReader sr = new StreamReader(st);
                resBodyStr = sr.ReadToEnd();
                st.Close();
                sr.Close();
                Twitch_TokenResponseDto resdata = JsonConvert.DeserializeObject<Twitch_TokenResponseDto>(resBodyStr);
                if (!string.IsNullOrEmpty(resdata.access_token) && !string.IsNullOrEmpty(resdata.refresh_token))
                {
                    // Customer更新
                    CustomerDao customerdao = new CustomerDao();
                    customerdao.UpdateCustomerTwitch(context.Value["twitter_id"].ToString(), resdata.access_token, resdata.refresh_token);
                }
				return true;
            }
			catch (HttpRequestException e)
			{
				return false;
			}
            catch (WebException e)
            {
                Console.WriteLine("webexception");
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse erres = (HttpWebResponse)e.Response;
                    Stream erst = erres.GetResponseStream();
                    StreamReader ersr = new StreamReader(erst);
                    resBodyStr = ersr.ReadToEnd();
                    ersr.Close();
                    erst.Close();
                    resStatusCode = erres.StatusCode;
                }
				return false;
            }
		}

		/// <summary>
		/// トークンの検証を行います
		/// </summary>
		/// <param name="accessToken"></param>
		/// <remarks>
		/// API: https://dev.twitch.tv/docs/authentication/validate-tokens
		/// curl: curl "https://id.twitch.tv/oauth2/validate" -H "Authorization: OAuth vwsfnr67uysueofiez37zx6funnc9q"
		/// </remarks>
		/// <returns></returns>
		public bool RequestValidateAccessToken(string twitter_id, string accessToken, string refresh_token)
		{
			// リクエスト内容定義
			string url = "https://id.twitch.tv/oauth2/validate";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.Headers.Add("Authorization", "OAuth " + accessToken);

            req.Timeout = 30 * 1000;

            string resBodyStr = string.Empty;
            HttpStatusCode resStatusCode = new HttpStatusCode();

            // リクエスト処理
            try
			{
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                Stream st = res.GetResponseStream();
                StreamReader sr = new StreamReader(st);
                resBodyStr = sr.ReadToEnd();
                st.Close();
                sr.Close();
                return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				RequestUpdateAccessToken(twitter_id, refresh_token);
                return false;
            }

			return false;
		}

        /// <summary>
        /// アクセストークンの更新を行います
        /// </summary>
        /// <param name="refresh_token"></param>
        /// <remarks>
        /// API: https://dev.twitch.tv/docs/authentication/refresh-tokens#how-to-use-a-refresh-token
        ///      https://dev.twitch.tv/docs/authentication/refresh-tokens
        /// curl: curl --request POST "https://id.twitch.tv/oauth2/token"
		///            -H "Content-Type: application/x-www-form-urlencoded"
		///            -d "client_id=wahx7ogta4k72inicil8t8po15uv4u"
		///            -d ""
        /// </remarks>
        /// <returns></returns>
        public bool RequestUpdateAccessToken(string twitter_id, string refresh_token)
		{
			// リクエスト内容定義
			string url = "https://id.twitch.tv/oauth2/token";

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			string param = string.Empty;

			param += "client_id=" + clientID;
			param += "&client_secret=" + secretToken;
			param += "&grant_type=refresh_token";
			param += "&refresh_token=" + HttpUtility.UrlEncode(refresh_token);

            req.Method = "POST";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            byte[] data = Encoding.ASCII.GetBytes(param);

            req.ContentLength = data.Length;

            req.Timeout = 30 * 1000;

            string resBodyStr = string.Empty;
            HttpStatusCode resStatusCode = HttpStatusCode.NotFound;

            Stream reqStream = req.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            // リクエスト処理
            try
			{
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                Stream st = res.GetResponseStream();
                StreamReader sr = new StreamReader(st);
                resBodyStr = sr.ReadToEnd();
                st.Close();
                sr.Close();
                Twitch_TokenResponseDto resdata = JsonConvert.DeserializeObject<Twitch_TokenResponseDto>(resBodyStr);
                if (!string.IsNullOrEmpty(resdata.access_token) && !string.IsNullOrEmpty(resdata.refresh_token))
                {
                    // Customer更新
                    CustomerDao customerdao = new CustomerDao();
                    customerdao.UpdateCustomerTwitch(twitter_id, resdata.access_token, resdata.refresh_token);
                }
                return true;

            }
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			return false;
		}

		/// <summary>
		/// 配信情報を取得します。
		/// </summary>
		/// <param name="access_token">アクセストークン</param>
		/// <remarks>
		/// API: https://dev.twitch.tv/docs/api/reference#get-streams
		/// </remarks>
		/// <returns></returns>
		public Twitch_HelixStreamDto RequestHelixStreams(MainContext context, string user_login)
		{
			string url = "https://api.twitch.tv/helix/streams";
			url += "?user_login=" + user_login;
			url += "&type=live";
			//url += "&first=100";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", "Bearer " + context.Value["twitch_accesstoken"].ToString());
			req.Headers.Add("Client-Id", clientID);

            req.Timeout = 30 * 1000;

            string resBodyStr = string.Empty;
            HttpStatusCode resStatusCode = new HttpStatusCode();

            try
            {
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                Stream st = res.GetResponseStream();
                StreamReader sr = new StreamReader(st);
                resBodyStr = sr.ReadToEnd();
                st.Close();
                sr.Close();
                Twitch_HelixStreamDto data = JsonConvert.DeserializeObject<Twitch_HelixStreamDto>(resBodyStr);
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse erres = (HttpWebResponse)e.Response;
                    Stream erst = erres.GetResponseStream();
                    StreamReader ersr = new StreamReader(erst);
                    resBodyStr = ersr.ReadToEnd();
                    ersr.Close();
                    erst.Close();
                    resStatusCode = erres.StatusCode;
                }
            }

            if (resBodyStr == null)
            {
                return null;
            }

            Twitch_HelixStreamDto resdata = JsonConvert.DeserializeObject<Twitch_HelixStreamDto>(resBodyStr);
            return resdata;
		}

		/// <summary>
		/// アクセストークンの取り消しを行います。
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool RequestRevokeAccessToken(MainContext context)
		{
			if (string.IsNullOrEmpty(context.Value["twitch_accesstoken"].ToString()))
			{
				return false;
			}

			// リクエスト内容定義
            string url = "https://id.twitch.tv/oauth2/revoke";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            string param = string.Empty;

            param += "client_id=" + clientID;
            param += "&token=" + context.Value["twitch_accesstoken"].ToString();

            req.Method = "POST";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            byte[] data = Encoding.ASCII.GetBytes(param);

            req.ContentLength = data.Length;

            req.Timeout = 30 * 1000;

            string resBodyStr = string.Empty;
            HttpStatusCode resStatusCode = HttpStatusCode.NotFound;

            Stream reqStream = req.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();

            // リクエスト処理
            try
            {
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                Stream st = res.GetResponseStream();
                StreamReader sr = new StreamReader(st);
                resBodyStr = sr.ReadToEnd();
                st.Close();
                sr.Close();
                CustomerDao dao = new CustomerDao();
				dao.DeleteCustomerTwitch(context.Value["customer"].ToString());
			}
			catch (WebException e)
			{
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse erres = (HttpWebResponse)e.Response;
                    Stream erst = erres.GetResponseStream();
                    StreamReader ersr = new StreamReader(erst);
                    resBodyStr = ersr.ReadToEnd();
                    ersr.Close();
                    erst.Close();
                    resStatusCode = erres.StatusCode;
                }
            }

			return false;
		}

		/// <summary>
		/// ランダム文字列を生成します
		/// </summary>
		/// <param name="length">文字列の長さ</param>
		/// <returns></returns>
		private string RandomString(int length)
		{
			if (length < 0) { return ""; }

			// ランダム文字列に使用する文字一覧
			string origin = "abcdefghijklmnopqrstuvwxyz01234789";

			Random random = new Random(32);

			string result = "";

			for (int i = 0; i < length; i++)
			{
				result += origin[random.Next(origin.Length)];
			}

			return result;
		}
	}
}
