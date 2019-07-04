using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Types
{
    public class AuthResult
    {
        public int Status { get; set; }
        public string Token { get; set; }
        public string ExpirationDate { get; set; }
    }
}
