using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.DependencyInjection.Services;
using System.Threading.Tasks;

namespace TwilioWhatsAppBot
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
            // create ContinuationActivity from the conversation reference.
            var activity = referenceActivity.GetConversationReference().GetContinuationActivity();
            var message = JsonConvert.SerializeObject(activity, jsonSettings);

            // Aend ResumeConversation event, it will get posted back to us with a specific value, giving us 
            // the ability to process it and do the right thing.
            await _queueService.SendJsonAsync(message, exchangeName: "exchange.name", routingKey: "routing.key");


        }
    }
}
