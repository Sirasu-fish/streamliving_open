namespace Main.Api.Twitter.Dto
{
	public class AuthorizationResponseDto
	{
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

		string? _code = string.Empty;
		public string? code
		{
			get => _code;
			set
			{
				_code = value;
			}
		}
	}
}
