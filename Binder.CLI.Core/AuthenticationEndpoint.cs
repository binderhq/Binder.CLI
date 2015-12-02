using Binder.API.Models.Authentication;
using Flurl;

namespace Binder.CLI.Core
{
    public class AuthenticationEndpoint
    {
        private readonly string _authenticationBaseUrl;

        public AuthenticationEndpoint(string authenticationBaseUrl)
        {
            _authenticationBaseUrl = authenticationBaseUrl;
        }





        public BinderSession CreateSession(string username, string clearTextPassword)
        {
            var req = new CreateSessionRequest()
            {
                ClearTextPassword = clearTextPassword,
                Username = username
            };

            var url = _authenticationBaseUrl.AppendPathSegment("Sessions");

            var responseMessage = new PostOperation<CreateSessionRequest>(url).WithRequest(req).ResponseMessage;

            return new BinderSession(responseMessage.Content<CreateSessionResponse>());


        }
    }
}
