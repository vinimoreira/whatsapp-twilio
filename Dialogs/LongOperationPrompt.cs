// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using TwilioWhatsAppBot.Factories;
using TwilioWhatsAppBot.Models;

namespace TwilioWhatsAppBot.Dialogs
{
    public class LongOperationPrompt : ActivityPrompt
    {

        /// <summary>
        /// Create a new instance of <see cref="LongOperationPrompt"/>.
        /// </summary>
        /// <param name="dialogId">Id of this <see cref="LongOperationPrompt"/>.</param>
        /// <param name="validator">Validator to use for this prompt.</param>
        public LongOperationPrompt(string dialogId, PromptValidator<Activity> validator)
            : base(dialogId, validator)
        {
        }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default)
        {
            // When the dialog begins, queue the option chosen within the Activity queued.
            //await _queueService.QueueActivityToProcess(dc.Context.Activity, (options as LongOperationPromptOptions).LongOperationOption, cancellationToken);
            return await base.BeginDialogAsync(dc, options, cancellationToken);
        }

        protected override Task<PromptRecognizerResult<Activity>> OnRecognizeAsync(ITurnContext turnContext, IDictionary<string, object> state, PromptOptions options, CancellationToken cancellationToken = default)
        {
            var result = new PromptRecognizerResult<Activity>() { Succeeded = false };

            if (turnContext.Activity.Type == ActivityTypes.Event
                && turnContext.Activity.Name == "ContinueConversation"
                && turnContext.Activity.Value != null)
            {
                result.Succeeded = true;
                result.Value = turnContext.Activity;
            }

            return Task.FromResult(result);
        }
    }
}
