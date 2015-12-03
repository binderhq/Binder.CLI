using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerArgs;

// ReSharper disable InconsistentNaming

namespace Binder.CLI
{
    public class UploadArguments 
    {
        [ArgRequired]
        [ArgDescription("Source filespec (must include a filename or a wildcard)")]
        [ArgExample(@"C:\MYFILES\*.*","")]
        public string source { get; set; }

        [ArgRequired]
        [ArgDescription("Destination folder in the site")]
        [ArgExample(@"/MyFolder/MyOtherFolder","")]
        public string destination { get; set; }

        [ArgDefaultValue(false)]
        [ArgDescription(@"Also upload all subdirectories and all files in those subdirectories")]
        public bool recursive { get; set; }

		[ArgDefaultValue(false)]
		[ArgDescription(@"Force upload to check all pieces instead of just checking timestamps")]
		public bool force { get; set; }
// ReSharper restore InconsistentNaming


    }
}
