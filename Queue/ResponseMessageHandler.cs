using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.DependencyInjection;
using RabbitMQ.Client.Core.DependencyInjection.MessageHandlers;
using RabbitMQ.Client.Core.DependencyInjection.Services;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwilioWhatsAppBot.CustomAdapter.TwilioWhatsApp;
using TwilioWhatsAppBot.Factories;
using TwilioWhatsAppBot.Models;
using TwilioWhatsAppBot.Utils;

namespace TwilioWhatsAppBot.Queue
{
    public class ResponseMessageHandler : INonCyclicMessageHandler
    {
        private readonly IBotFrameworkHttpAdapter _adapterBoot;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private IQueueService _queueService;
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };


        public ResponseMessageHandler(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _adapterBoot = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
        }

        public void Handle(BasicDeliverEventArgs eventArgs, string matchingRoute, IQueueService queueService)
        {
            _queueService = queueService;
            var payload = eventArgs.GetPayload<Response>();

            ConversationReference conversation;
            _conversationReferences.TryGetValue(payload.ReplyToId, out conversation);

            if (conversation == null)
                throw new InvalidOperationException("Não foi possível localizar o usuário de retorno");

            AddResponseInDialog(conversation, payload);
        }

        private async Task AddResponseInDialog(ConversationReference conversation, Response reponse)
        {

            var questionId = (int?)conversation.Conversation.Properties["NextQuestion"];
            var actualStep = QuizFactory.GetQuiz(questionId);
            var nextStep = QuizFactory.GetQuiz(actualStep.NextQuestion);

            if (reponse.Code == 200)
                SendMessageResponse(nextStep, reponse);
            else
                SendErrorMessageResponse(actualStep, reponse);

        }

        private void SendMessageResponse(Quiz quiz, Response reponse)
        {
            if (quiz.Messages != null)
            {
                foreach (var message in quiz.Messages.ToList().OrderBy(x => x.Order))
                {
                    string text = message.Type == "template" ? HtmlRenderer.Render(message.Text, string.Format("{0}-{1}", quiz.Id, message.Order), reponse.Payload) : message.Text;

                    SendMessageQueue(new Question()
                    {
                        Id = quiz.Id,
                        End = quiz.End,
                        NextQuestion = message.NextQuestion ?? quiz.NextQuestion,
                        Options = message.Options,
                        ReplyToId = reponse.ReplyToId,
                        Text = text,
                        Type = message.Type
                    });
                }
            }
        }

        private void SendErrorMessageResponse(Quiz quiz, Response reponse)
        {
            SendMessageQueue(new Question() { Id = quiz.Id, ReplyToId = reponse.ReplyToId, Text = reponse.Payload.ErrorMessage, Type = "text", NextQuestion = quiz.Id });

            SendMessageQueue(new Question()
            {
                Id = quiz.Id,
                End = quiz.End,
                NextQuestion = quiz.Id,
                ReplyToId = reponse.ReplyToId,
                Text = quiz.RetryText,
                Type = quiz.Type
            });
        }

        public void SendMessageQueue(Question question)
        {
            var message = JsonConvert.SerializeObject(question, jsonSettings);
            _queueService.SendJsonAsync(message, exchangeName: "bot.dialog", routingKey: "dialog.key");
        }

    }
}
