using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerArgs;

namespace Binder.CLI
{
    public class UploadArguments 
    {
        [ArgRequired]
        public string source { get; set; }

        [ArgRequired]
        public string destination { get; set; }

       


    }
}
