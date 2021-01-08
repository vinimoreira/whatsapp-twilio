using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwilioWhatsAppBot.Models
{
    public class Question
    {
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
