using System.Collections.Generic;

namespace TwilioWhatsAppBot.Models
{
    public class Question
    {
        public string ReplyToId { get; set; }
        public int Id { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public int? NextQuestion { get; set; }
        public List<Options> Options { get; set; }
        public bool End { get; set; }
    }

    public class Options
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int? NextQuestion { get; set; }
    }
}
