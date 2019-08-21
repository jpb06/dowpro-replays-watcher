using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Types
{
    public class RelicChunkyData
    {
        public string ModName { get; set; }
        public string MapName { get; set; }
        public string ReplayName { get; set; }

        public long DatabaseChunkLengthPosition { get; set; }
        public long ReplayNameLengthPosition { get; set; }
        public long ReplayNameValueEndPosition { get; set; }
        public long HeaderLengthPosition { get; set; }

        public int DatabaseChunkLength { get; set; }
        public int HeaderLength { get; set; }
    }
}
