using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.DependencyInjection.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwilioWhatsAppBot.Factories;
using TwilioWhatsAppBot.Models;

namespace TwilioWhatsAppBot.Queue
{
    public class BotQueueService
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        private readonly IQueueService _queueService;

        public BotQueueService(IQueueService queueService)
        {
            _queueService = queueService;
        }

        public async Task ResponseActivityToProcess(IMessageActivity referenceActivity)
        {
            var nextQuestionId = (int?)referenceActivity.Conversation.Properties["NextQuestion"];
            var questionId = (int?)referenceActivity.Conversation.Properties["QuestionId"];
            var value = referenceActivity.Value != null ? referenceActivity.Value.ToString() : referenceActivity.Text;

            var quiz = GetQuiz(questionId, nextQuestionId, value);

            if (quiz.Type == "question")
            {
                if (quiz.Messages != null)
                {
                    foreach (var message in quiz.Messages.ToList().OrderBy(x => x.Order))
                    {
                        await Task.Delay(1000);
                        SendQuestion(new Question()
                        {
                            Id = quiz.Id,
                            End = quiz.End,
                            NextQuestion = message.NextQuestion ?? quiz.NextQuestion,
                            Options = message.Options,
                            ReplyToId = referenceActivity.From.Id,
                            Text = message.Text,
                            Type = message.Type
                        });
                    }
                }
            }
            else
            {
                var answer = new Answer()
                {
                    Id = referenceActivity.Id,
                    ChannelId = referenceActivity.ChannelId,
                    FromId = referenceActivity.From.Id,
                    Text = referenceActivity.Text,
                    Type = referenceActivity.Type,
                    Value = referenceActivity.Value,
                    QuestionId = (int?)referenceActivity.Conversation.Properties["QuestionId"]
                };

                var message = JsonConvert.SerializeObject(answer, jsonSettings);
                await _queueService.SendJsonAsync(message, exchangeName: "integration.request", routingKey: "request.key");
            }
        }

        private void SendQuestion(Question question)
        {
            var message = JsonConvert.SerializeObject(question, jsonSettings);
            _queueService.SendJsonAsync(message, exchangeName: "bot.dialog", routingKey: "dialog.key");
        }

        private Quiz GetQuiz(int? questionIdActual, int? nextQuestion, string text)
        {

            if (!questionIdActual.HasValue)
                return QuizFactory.GetQuiz(questionIdActual);

            if (nextQuestion.HasValue)
                return QuizFactory.GetQuiz(nextQuestion);

            return GetQuizFromOption(questionIdActual, text);
        }

        private Quiz GetQuizFromOption(int? questionIdActual, string text)
        {
            var quizActual = QuizFactory.GetQuiz(questionIdActual);
            var questionOption = quizActual.Messages.FirstOrDefault(x => x.Type == "options");
            var selectedOption = questionOption.Options.FirstOrDefault(x => x.Text == text || x.Id.ToString() == text);

            if (selectedOption == null)
                return GetOptionNotFound(quizActual);

            return QuizFactory.GetQuiz(selectedOption.NextQuestion);
        }

        private Quiz GetOptionNotFound(Quiz quizActual)
        {
            var messages = new List<Message>();
            var questionOption = quizActual.Messages.FirstOrDefault(x => x.Type == "options");

            messages.Add(new Message() { Text = questionOption.NotFoundText, Order = 0, Type = "text" });
            messages.Add(questionOption);

            return new Quiz()
            {
                End = quizActual.End,
                Id = quizActual.Id,
                Type = quizActual.Type,
                Messages = messages
            };
        }

    }
}
