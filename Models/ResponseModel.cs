using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TwilioWhatsAppBot.Models
{
    public class Response
    {
        public string Id { get; set; }
        public string ReplyToId { get; set; }
        public int Code { get; set; }
        public dynamic Payload { get; set; }
    }
}