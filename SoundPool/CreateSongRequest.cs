using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SoundPool
{
    [DataContract]
    public class CreateSongRequest
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Artist { get; set; }

        [DataMember]
        public string Url { get; set; }
    }
}