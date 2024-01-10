// https://dev.twitch.tv/docs/authentication/validate-tokens

namespace Main.Api.Twitch.Dto
{
    public class Twitch_ValidateOauth2
    {
        public string? _client_id = string.Empty;
        public string? client_id
        {
            get
            {
                return _client_id;
            }
            set
            {
                _client_id = value;
            }
        }

        public string? _login = string.Empty;
        public string? login
        {
            get
            {
                return _login;
            }
            set
            {
                _login = value;
            }
        }

        public string? _user_id = string.Empty;
        public string? user_id
        {
            get
            {
                return _user_id;
            }
            set
            {
                _user_id = value;
            }
        }

        public string? _expires_in = string.Empty;
        public string? expires_in
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
    }
}
