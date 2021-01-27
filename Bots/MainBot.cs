// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TwilioWhatsAppBot.Models;

namespace TwilioWhatsAppBot.Bots
{
    public class MainBot : ActivityHandler
    {
        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
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
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = MessageFactory.Text($"Welcome to Complex Dialog Bot {member.Name}. " +
                        "This bot provides a complex conversation, with multiple dialogs. " +
                        "Type anything to get started.");
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }

        protected override async Task OnEventActivityAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            // The event from the Azure Function will have a name of 'LongOperationResponse' 
            if (turnContext.Activity.Name == "NotifyResponse")
            {
                // The response will have the original conversation reference activity in the .Value
                // This original activity was sent to the Azure Function via Azure.Storage.Queues in AzureQueuesService.cs.
                var conversationReference = _conversationReferences[turnContext.Activity.ReplyToId];
                await turnContext.Adapter.ContinueConversationAsync(_appId, conversationReference, async (context, cancellation) =>
                {
                    //Logger.LogInformation("Running dialog with Activity from LongOperationResponse.");
                    await ResponseQuestionUser(turnContext, cancellation);

                }, cancellationToken);
            }
            else
            {
                await base.OnEventActivityAsync(turnContext, cancellationToken);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            // Save any state changes.
            await _queueService.QueueActivityToProcess(turnContext.Activity);
            await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        private async Task ResponseQuestionUser(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            var question = (Question)turnContext.Activity.Value;

            var reply = MessageFactory.Text(question.Text);

            if (question.Options != null)
            {
                reply.SuggestedActions = new SuggestedActions()
                {
                    Actions = question.Options.Select(opt => new CardAction() { Title = opt.Text, Text = opt.Text, Value = opt.Id, DisplayText = opt.Text, Type = ActionTypes.MessageBack }).ToList()
                };
            }


            await turnContext.SendActivityAsync(reply, cancellationToken);

            // Save any state changes that might have occurred during the inner turn.
            //await ConversationState.SaveChangesAsync(context, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

    }
}