using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Binder.API.Models.Authentication;

namespace Binder.CLI.Core
{
    public class BinderSession
    {
        private readonly CreateSessionResponse _createSessionResponse;

        public BinderSession(CreateSessionResponse createSessionResponse)
        {
            _createSessionResponse = createSessionResponse;
        }

        public object SessionToken
        {
            get { return _createSessionResponse.SessionToken; }
        }
    }
}
