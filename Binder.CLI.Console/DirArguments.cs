using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PowerArgs;

namespace Binder.CLI
{
    public class DirArguments
    {
        [ArgRequired]
        public string source { get; set; }

    }
}
