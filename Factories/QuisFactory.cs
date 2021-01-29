using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TwilioWhatsAppBot.Models;

namespace TwilioWhatsAppBot.Factories
{
    public static class QuisFactory
    {
        public static List<Question> GetQuestions()
        {
            var paths = new[] { ".", "Data", "questionario.json" };
            var quizJsonString = File.ReadAllText(Path.Combine(paths));
            
            return JsonConvert.DeserializeObject<List<Question>>(quizJsonString);
        }
    }
}
