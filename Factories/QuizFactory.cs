using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwilioWhatsAppBot.Models;

namespace TwilioWhatsAppBot.Factories
{
    public static class QuizFactory
    {
        private static List<Quiz> _fullQuiz = FullQuiz();

        static List<Quiz> FullQuiz()
        {
            var paths = new[] { ".", "Data", "questionario.json" };
            var quizJsonString = File.ReadAllText(Path.Combine(paths));

            return JsonConvert.DeserializeObject<List<Quiz>>(quizJsonString);
        }

        public static IReadOnlyList<Quiz> GetFullQuiz()
        {
            return _fullQuiz;
        }

        public static Quiz GetQuiz(int? quizId)
        {
            if (!quizId.HasValue) quizId = 1;

            return _fullQuiz.FirstOrDefault(x => x.Id == quizId);
        }
    }
}
