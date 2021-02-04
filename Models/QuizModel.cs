using System.Collections.Generic;

namespace TwilioWhatsAppBot.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public List<Message> Messages { get; set; }
        public int? NextQuestion { get; set; }
        public int? ErrorQuestion { get; set; }
        public bool End { get; set; }
        public string RetryText { get; set; }

    }

    public class Message
    {
        public string Text { get; set; }
        public string Type { get; set; }
        public List<Options> Options { get; set; }
        public int? NextQuestion { get; set; }
        public int? Order { get; set; }
        public string NotFoundText { get; set; }
    }
}
