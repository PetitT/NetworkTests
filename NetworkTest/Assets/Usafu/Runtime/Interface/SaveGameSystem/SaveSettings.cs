using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingCactus.SaveGameSystem
{
    public class SaveSettings
    {
        public PS4Data PS4 = new PS4Data();

        public struct PS4Data
        {
            /// <summary>
            /// The maximum size this save slot can take up, in bytes.
            /// </summary>
            public ulong MaxSize;
            public byte[] Icon;
            public string Title;
            public string SubTitle;
            public string Detail;
        }
    }
}
