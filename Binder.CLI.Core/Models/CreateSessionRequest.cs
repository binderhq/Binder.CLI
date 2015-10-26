namespace Binder.API.Models.Authentication
{
	public class CreateSessionRequest  
	{
		public string Username { get; set; }
		public string ClearTextPassword { get; set; }

	}

	
}