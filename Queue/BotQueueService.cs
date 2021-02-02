using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.DependencyInjection.Services;
using System.Threading.Tasks;
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

        public async Task QueueActivityToProcess(IMessageActivity referenceActivity)
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

            // Aend ResumeConversation event, it will get posted back to us with a specific value, giving us 
            // the ability to process it and do the right thing.
            await _queueService.SendJsonAsync(message, exchangeName: "exchange.name", routingKey: "answer.key");


        }
    }
}
