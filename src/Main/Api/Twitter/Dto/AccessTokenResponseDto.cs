namespace Main.Api.Twitter.Dto
{
    public class AccessTokenResponseDto
    {
        string? _token_type = string.Empty;
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

        string? _refresh_token = string.Empty;
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
