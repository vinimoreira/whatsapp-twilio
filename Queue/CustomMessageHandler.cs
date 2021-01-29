using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Core.DependencyInjection;
using RabbitMQ.Client.Core.DependencyInjection.MessageHandlers;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TwilioWhatsAppBot.Models;

namespace TwilioWhatsAppBot.Queue
{
    public class CustomMessageHandler : IMessageHandler
    {
        readonly ILogger<CustomMessageHandler> _logger;
        private IBot _bot;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public CustomMessageHandler(IBotFrameworkHttpAdapter adapter, ILogger<CustomMessageHandler> logger, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _logger = logger;
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
        }

        public void Handle(BasicDeliverEventArgs eventArgs, string matchingRoute)
        {
            var payload = eventArgs.GetPayload<Question>();
            ConversationReference conversation = null;
            _conversationReferences.TryGetValue(payload.ReplyToId, out conversation);
            ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversation, async (context, token) => await BotCallback(payload, context, token), default(CancellationToken));
        }

        private async Task BotCallback(Question question, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text(question.Text);

            if (question.Options != null)
            {
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = question.Options.Select(opt => new CardAction() { Title = opt.Text, Text = opt.Text, Value = opt.Id, DisplayText = opt.Text, Type = ActionTypes.MessageBack }).ToList()
                };
            }

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

    }
}
