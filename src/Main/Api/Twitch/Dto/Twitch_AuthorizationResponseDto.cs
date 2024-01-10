// https://dev.twitch.tv/docs/authentication/getting-tokens-oauth#authorization-code-grant-flow
namespace Main.Api.Twitch.Dto
{
    public class Twitch_AuthorizationResponseDto
    {
        // 承認時
        string? _code = string.Empty;
        public string? code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
            }
        }

        // 承認時、非承認時
        string? _scope = string.Empty;
        public string? scope
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

        // 承認時
		string? _state = string.Empty;
        public string? state
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        // 非承認時
		string? _error = string.Empty;
        public string? error
        {
            get
            {
                return _error;
            }
            set
            {
                _error = value;
            }
        }

        // 非承認時
        string? _error_description = string.Empty;
        public string? error_description
        {
            get
            {
                return _error_description;
            }
            set
            {
                _error_description = value;
            }
        }
	}
}
