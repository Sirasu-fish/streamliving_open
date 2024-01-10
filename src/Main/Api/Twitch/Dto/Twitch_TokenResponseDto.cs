// https://dev.twitch.tv/docs/authentication/getting-tokens-oauth#authorization-code-grant-flow
namespace Main.Api.Twitch.Dto
{
	public class Twitch_TokenResponseDto
	{
		string? _access_token = string.Empty;
		public string? access_token
		{
			get
			{
				return _access_token;
			}
			set
			{
				_access_token = value;
			}
		}

		int? _expires_in = 0;
		public int? expires_in
		{
			get
			{
				return _expires_in;
			}
			set
			{
				_expires_in = value;
			}
		}

		string _refresh_token = string.Empty;
		public string? refresh_token
		{
			get
			{
				return _refresh_token;
			}
			set
			{
				_refresh_token = value;
			}
		}

		string[]? _scope = new string[99];
		public string[]? scope
		{
			get
			{
				return _scope;
			}
            set
            {
                _scope = value;
            }
		}

		string _token_type = string.Empty;
		public string? token_type
		{
			get
			{
				return _token_type;
			}
			set
			{
				_token_type = value;
			}
		}
	}
}
