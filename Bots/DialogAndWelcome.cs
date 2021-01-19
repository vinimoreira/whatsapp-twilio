// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace TwilioWhatsAppBot.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T> where T : Dialog
    {
        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        protected readonly IStatePropertyAccessor<DialogState> DialogState;
        private readonly string _appId;

        public DialogAndWelcomeBot(IConfiguration configuration, ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, ConcurrentDictionary<string, ConversationReference> conversationReferences)
            : base(conversationState, userState, dialog, logger)
        {
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            DialogState = ConversationState.CreateProperty<DialogState>(nameof(DialogState));
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
                    Logger.LogInformation("Running dialog with Activity from LongOperationResponse.");

                    // ContinueConversationAsync resets the .Value of the event being continued to Null, 
                    //so change it back before running the dialog stack. (The .Value contains the response 
                    //from the Azure Function)
                    context.Activity.Value = turnContext.Activity.Value;
                    await Dialog.RunAsync(context, DialogState, cancellationToken);

                    // Save any state changes that might have occurred during the inner turn.
                    await ConversationState.SaveChangesAsync(context, false, cancellationToken);
                }, cancellationToken);
            }
            else
            {
                await base.OnEventActivityAsync(turnContext, cancellationToken);
            }
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, DialogState, cancellationToken);
        }
    }
}