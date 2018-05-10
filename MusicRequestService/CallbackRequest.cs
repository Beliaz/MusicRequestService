using System.Runtime.Serialization;

namespace MusicRequestService
{
    [DataContract]
    public class CallbackRequest
    {
        [DataMember]
        public string Uri { get; set; }

        [DataMember]
        public string FileName { get; set; }
    }
}