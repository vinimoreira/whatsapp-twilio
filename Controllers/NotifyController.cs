// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using TwilioWhatsAppBot.Models;

namespace TwilioWhatsAppBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences, IBot bot)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _bot = bot;
        }

        [HttpPost("{id}")]
        public async Task PostAsync(string id, Question question)
        {
            //Delegate the processing of the HTTP POST to the adapter.
            //The adapter will invoke the bot.
            //var conversationReference = _conversationReferences;

            foreach (var conversationReference in _conversationReferences)
            {

                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference.Value, async (context, token) => await BotCallback(conversationReference.Key, question, context, token), default(CancellationToken));
            }

            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            //PostActivityAsync(conversation.ConversationId, responseActivity);

        }

        private async Task BotCallback(string id,Question question, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            //If you encounter permission-related errors when sending this message, see
            //https://aka.ms/BotTrustServiceUrl

            var responseActivity = new Activity("event");
            responseActivity.Value = question;
            responseActivity.Name = "NotifyResponse";
            responseActivity.From = new ChannelAccount("GenerateReport", "AzureFunction");

            turnContext.Activity.Name = "NotifyResponse";
            turnContext.Activity.Value = question;
            turnContext.Activity.ReplyToId = id;

            await _bot.OnTurnAsync(turnContext, cancellationToken);

            //((BotAdapter)_adapter).SendActivitiesAsync()

            //await turnContext.SendActivityAsync(responseActivity, cancellationToken);
        }
    }
}