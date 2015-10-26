using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binder.CLI.Core
{
    public class Site
    {
        private readonly Region _region;
        private readonly string _subdomain;
        private readonly BinderSession _session;

        public Site(Region region, string subdomain, BinderSession session)
        {
            _region = region;
            _subdomain = subdomain;
            _session = session;
        }

        public Region Region
        {
            get { return _region; }
        }

        public string Subdomain
        {
            get { return _subdomain; }
        }
    }
}
