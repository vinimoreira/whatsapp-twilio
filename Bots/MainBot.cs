// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TwilioWhatsAppBot.Queue;

namespace TwilioWhatsAppBot.Bots
{
    public class MainBot : ActivityHandler
    {
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly BotState _userState;
        private readonly string _appId;
        protected readonly ILogger Logger;
        private readonly BotQueueService _queueService;

        public MainBot(IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences, UserState userState, ILogger<MainBot> logger, BotQueueService queueService)
        {
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _userState = userState;
            _queueService = queueService;
            Logger = logger;
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();

            ConversationReference conversation = null;
            _conversationReferences.TryGetValue(conversationReference.User.Id, out conversation);

            if (conversation != null)
                conversationReference.Conversation.Properties = conversation.Conversation.Properties;

            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);
            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

       
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new message Activity.
            // Save any state changes.
            AddConversationReference(turnContext.Activity as Activity);
            await _queueService.QueueActivityToProcess(turnContext.Activity);
            await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

    }
}