namespace Binder.API.Models.Authentication
{
    public class CreateSessionResponse
    {
        public string SessionToken { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }

        public string _EdocxUserId { get; set; }
        public string _NetworkName { get; set; }
        public string _NetworkId { get; set; }
        public string _NetworkPayload { get; set; }

    }
}