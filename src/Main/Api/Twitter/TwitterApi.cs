using Azure;
using Azure.Core;
using Main.Api.Twitter.Dto;
using Main.Data;
using Main.Models.Dao;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Main.Api.Twitch
{
	public class TwitterApi
	{
		string oauth_consumer_key = "xxxxx"; // 認証キー 多分25文字
		string oauth_consumer_secret = ""; // 認証シークレットキー 多分50文字
		string client_id = "xxxxx"; // クライアントID
		string client_secret = "xxxxx"; // シークレット
		string redirecturi = "https://localhost/main/home/Twitter_GetAccessToken";

        /// <summary>
        /// TwitterでログインボタンのURLを作成します。
        /// </summary>
        /// <remarks>
        /// API: https://developer.twitter.com/en/docs/authentication/oauth-2-0/authorization-code
        ///      https://developer.twitter.com/en/docs/authentication/oauth-2-0/user-access-token Step1
        /// 参考: https://qiita.com/YouKnow/items/52b62d5487fa45df110c
        ///       https://qiita.com/kunihiros/items/2722d690b1525813c45e
        /// ログイン画面
        /// </remarks>
        /// <returns></returns>
        public string CreateAuthorizationUrl(MainContext context)
		{
			string url = "https://twitter.com/i/oauth2/authorize";
			string state = RandomString(64);
			string code_challenge = RandomString(64);

			// state, code_challengeはsessionに保存
			SessionDao dao = new SessionDao();

            SHA256CryptoServiceProvider sha = new SHA256CryptoServiceProvider();
            byte[] data = Encoding.ASCII.GetBytes(code_challenge);
            string code_challenge_sha = Convert.ToBase64String(sha.ComputeHash(data));

            dao.UpdateSessionForTwitter(context, state, code_challenge);

            url += "?response_type=code";
			url += "&client_id=" + client_id;
			url += "&redirect_uri=" + redirecturi;
			url += "&scope=tweet.read%20users.read%20offline.access";
			url += "&state=" + state;
            url += "&code_challenge=" + Convert.ToBase64String(sha.ComputeHash(data)).Replace("+", "-").Replace("/", "_").Replace("=", "");
			url += "&code_challenge_method=S256";

			return url;
		}

        /// <summary>
        /// アクセストークンを取得するリクエストを送信します。
        /// </summary>
        /// <remarks>
        /// API: https://developer.twitter.com/en/docs/authentication/oauth-2-0/user-access-token
        /// curl: curl -X POST
        ///       https://api.twitter.com/2/oauth2/token
        ///       --basic -u "xxxxx:xxxxx"
        ///       --header "Content-Type: application/x-www-form-urlencoded"
        ///       --data-urlencode "grant_type=authorization_code"
        ///       --data-urlencode "redirect_uri=https://localhost/main/home/Twitter_GetAccessToken"
        ///       --data-urlencode "code_verifier=xxxxxx"
        ///       --data-urlencode "client_id=xxxxx"
        ///       --data-urlencode "code="xxxxx"
        /// 参考: https://qiita.com/YouKnow/items/52b62d5487fa45df110c
        /// Twitter アプリにアクセスを許可後、POSTが必要
        /// </remarks>
        /// <returns></returns>
        public bool RequestAccessToken(ref MainContext context, string code)
		{
            // リクエスト内容
			string url = "https://api.twitter.com/2/oauth2/token";

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "POST";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
			req.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(client_id + ":" + client_secret)));

			string body = string.Empty;
			body += "grant_type=authorization_code";
			body += "&redirect_uri=" + HttpUtility.UrlEncode(redirecturi);
			body += "&code_verifier=" + context.Value["twitter_code_challenge"].ToString();
			body += "&client_id=" + client_id;
			body += "&code=" + code;
			byte[] data = Encoding.ASCII.GetBytes(body);

			req.ContentLength = data.Length;

			req.Timeout = 30*1000;

			string resBodyStr = string.Empty;
			HttpStatusCode resStatusCode = new HttpStatusCode();

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
			}
			catch (HttpRequestException e)
			{
				resStatusCode = (HttpStatusCode)e.StatusCode;
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
            AccessTokenResponseDto resdata = JsonConvert.DeserializeObject<AccessTokenResponseDto>(resBodyStr);

            string loginsession = string.Empty;

			if (!string.IsNullOrEmpty(resdata.access_token) && !string.IsNullOrEmpty(resdata.refresh_token))
			{
                string userid = RequestUserLoopUp(resdata.access_token);
                if (!string.IsNullOrEmpty(userid))
                {
                    // Customer登録
                    CustomerDao customerdao = new CustomerDao();
                    customerdao.InsertCustomer(userid, resdata.access_token, resdata.refresh_token);
                    context.Value["twitter_id"] = userid;
                    context.Value["loginsession"] = RandomString(64);
                    SessionDao sessiondao = new SessionDao();
                    sessiondao.UpdateSessionTwitterId(context);

                }
            }

			return true;
		}

        /// <summary>
        /// TwitterのUserIDを取得します。取得できない場合はEmptyを返します。
        /// </summary>
        /// <param name="context"></param>
        /// <returns>例:884207361400815616</returns>
        public string RequestUserLoopUp(string access_token)
        {
            string url = "https://api.twitter.com/2/users/me";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", "Bearer " + access_token);

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
            Twitter_UserMeDto resdata = JsonConvert.DeserializeObject<Twitter_UserMeDto>(resBodyStr);

            return resdata.data.id;
        }

        /// <summary>
        /// トークンが正常か確認します。
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>
        /// API: https://developer.twitter.com/en/docs/twitter-api/users/lookup/api-reference/get-users-me
		/// curl: -H "Authorization: Bearer XXXXXXX"
		///       "https://api.twitter.com/2/oauth2/token"
        /// </remarks>
        /// <returns></returns>
        public bool RequestCheckUser(MainContext context)
		{
            string url = "https://api.twitter.com/2/users/me";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", "Bearer " + context.Value["twitter_accesstoken"].ToString());

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
            Twitter_UserMeDto resdata = JsonConvert.DeserializeObject<Twitter_UserMeDto>(resBodyStr);

            if (resdata == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// トークンの更新を行います。
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>
        /// API: https://developer.twitter.com/en/docs/authentication/oauth-2-0/user-access-token
        /// curl: curl --request POST "https://api.twitter.com/2/oauth2/token"
		///            -H "Authorization: Basic xxxxx"
        ///            -H "Content-Type: application/x-www-form-urlencoded"
        ///            --data-urlencode "refresh_token=YlNFdUd5RGtFaXpxcV9fQ2pGblhORVpjbWtxaDhCaEtfY2dROS1aS1RSUWFXOjE2NzI0OTkzOTkwMjE6MToxOnJ0OjE"
		///            --data-urlencode "grant_type=refresh_token"
		///            --data-urlencode "client_id=xxxxx"
        /// </remarks>
        /// <returns></returns>
        public bool RequestUpdateAccessToken(string Id, string twitter_refreshtoken)
		{
           string url = "https://api.twitter.com/2/oauth2/token";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "POST";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(client_id + ":" + client_secret)));

            string body = string.Empty;
            body += "grant_type=refresh_token";
            body += "&client_id=" + client_id;
            body += "&refresh_token=" + twitter_refreshtoken;
            byte[] data = Encoding.ASCII.GetBytes(body);

            req.ContentLength = data.Length;

            req.Timeout = 30 * 1000;

            string resBodyStr = string.Empty;
            HttpStatusCode resStatusCode = new HttpStatusCode();

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
                AccessTokenResponseDto resdata = JsonConvert.DeserializeObject<AccessTokenResponseDto>(resBodyStr);
                // Customer更新
                CustomerDao customerdao = new CustomerDao();
                customerdao.UpdateCustomerTwitter(Id, resdata.access_token, resdata.refresh_token);
            }
            catch (HttpRequestException e)
            {
                resStatusCode = (HttpStatusCode)e.StatusCode;
                return false;
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
                return false;
            }

            return true;
		}


		/// <summary>
		/// 連携解除処理を行います。
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public bool RequestRevokeAccessToken(MainContext context)
		{
            string url = "https://api.twitter.com/2/oauth2/revoke";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "POST";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            string body = string.Empty;
            body += "token=" + context.Value["twitter_accesstoken"];
			body += "&client_id=" + client_id;
            byte[] data = Encoding.ASCII.GetBytes(body);

            req.ContentLength = data.Length;

            req.Timeout = 30 * 1000;

            string resBodyStr = string.Empty;
            HttpStatusCode resStatusCode = new HttpStatusCode();

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
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("HttpRequestException");
                resStatusCode = (HttpStatusCode)e.StatusCode;
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
            }
            AccessTokenResponseDto resdata = JsonConvert.DeserializeObject<AccessTokenResponseDto>(resBodyStr);

            if (resdata == null)
            {
                return true;
            }
            if (!string.IsNullOrEmpty(resdata.access_token) && !string.IsNullOrEmpty(resdata.refresh_token))
            {
				// Sessionログアウト
				SessionDao dao = new SessionDao();
				dao.LogoutSession(context);
            }

            return true;
        }

        /// <summary>
        /// TwitterのIDを名前から取得します。(@say_matane → 884207361400815616)
        /// </summary>
        /// <param name="userName"></param>
        /// <remarks>
        /// API: https://developer.twitter.com/en/docs/twitter-api/users/lookup/api-reference/get-users-by-username-username
        /// </remarks>
        /// <returns></returns>
        public string RequestUserIdFromUserName(string access_token, string userName)
        {
            string url = "https://api.twitter.com/2/users/by/username/" + userName;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", "Bearer " + access_token);

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
            Twitter_UserMeDto resdata = JsonConvert.DeserializeObject<Twitter_UserMeDto>(resBodyStr);

            if (resdata != null && resdata.data != null && !string.IsNullOrEmpty(resdata.data.id))
            {
                return resdata.data.id;
            }

            return string.Empty;
        }

        /// <summary>
        /// ツイート情報を取得します。
        /// </summary>
        /// <param name="access_token">アクセストークン</param>
        /// <param name="id">Twitter UserID(例：884207361400815616)</param>
        /// <param name="num">ツイート取得件数</param>
        /// <remarks>
        /// API: https://developer.twitter.com/en/docs/twitter-api/tweets/timelines/api-reference/get-users-id-tweets
        /// 参考: https://zenn.dev/yoiyoicho/articles/16dd8410da97e9
        /// </remarks>
        /// <returns></returns>
        public Twitter_TweetsDto RequestStreamerTweets(string access_token, string id, int num)
        {
            string url = "https://api.twitter.com/2/users/" + id + "/tweets";

            // url += "end_time="
            // 返信は除去
            url += "?exclude=replies";
            // https://developer.twitter.com/en/docs/twitter-api/expansions
            //
            url += "&expansions=attachments.poll_ids" +
                   ",attachments.media_keys" +
                   ",author_id" +
                   // ",edit_history_tweet_ids" +
                   ",entities.mentions.username" +
                   ",geo.place_id" +
                   ",in_reply_to_user_id" +
                   //",referenced_tweets.id" +
                   ",referenced_tweets.id.author_id";
            url += "&max_results=" + num;
            // https://developer.twitter.com/en/docs/twitter-api/data-dictionary/object-model/media
            url += "&media.fields=media_key" +
                   ",url" +
                   ",height" +
                   ",width" +
                   ",alt_text";
            // url += "pagination_token="
            // url += "&place.fields="
            // https://developer.twitter.com/en/docs/twitter-api/data-dictionary/object-model/poll
            url += "&poll.fields=id" +
                   ",options";
            // url += "since_id="
            // url += "start_time="
            // https://developer.twitter.com/en/docs/twitter-api/data-dictionary/object-model/tweet
            url += "&tweet.fields=attachments" +
                   ",author_id" +
                   ",referenced_tweets" +
                   ",created_at" +
                   ",public_metrics";
            // url += "until_id="
            url += "&user.fields=id" +
                   ",name" +
                   ",pinned_tweet_id" +
                   ",profile_image_url" +
                   ",url" +
                   ",username";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", "Bearer " + access_token);

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

            Twitter_TweetsDto resdata = JsonConvert.DeserializeObject<Twitter_TweetsDto>(resBodyStr);
            return resdata;
        }

        /// <summary>
        /// 引用ツイートの元ツイート情報を取得します。
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>
        /// API: https://developer.twitter.com/en/docs/twitter-api/tweets/quote-tweets/quick-start
        /// </remarks>
        /// <returns></returns>
        public string RequestQuoteTweets(string access_token, string id)
        {
            // https://api.twitter.com/2/tweets/:id/quote_tweets?expansions=author_id&tweet.fields=created_at&user.fields=created_at
            string url = "https://api.twitter.com/2/tweets/" + id + "/quote_tweets";
            url += "?expansion=author_id";
            url += "&tweet.fields=created_at";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "GET";
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", "Bearer " + access_token);

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

            return resBodyStr;
        }



        #region private

        /// <summary>
        /// ランダム文字列を生成します
        /// </summary>
        /// <param name="length">文字列の長さ</param>
        /// <returns></returns>
        private string RandomString(int length)
		{
			if (length < 0) { return ""; }

			// ランダム文字列に使用する文字一覧
			string origin = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234789";

			Random random = new Random();

			string result = "";

			for (int i = 0; i < length; i++)
			{
				result += origin[random.Next(origin.Length)];
			}

			return result;
		}

		#endregion private
	}
}