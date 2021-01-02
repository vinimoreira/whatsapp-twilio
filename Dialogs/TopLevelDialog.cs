// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using TwilioWhatsAppBot;

namespace TwilioWhatsAppBot.Dialogs
{
    public class TopLevelDialog : ComponentDialog
    {
        // Define a "done" response for the company selection prompt.
        private const string DoneOption = "done";

        // Define value names for values tracked inside the dialogs.
        private const string UserInfo = "value-userInfo";

        public TopLevelDialog()
            : base(nameof(TopLevelDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));

            AddDialog(new TimeSelectionDialog());

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NameStepAsync,
                AgeStepAsync,
                StartSelectionStepAsync,
                AcknowledgementStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create an object in which to collect the user's information within the dialog.
            stepContext.Values[UserInfo] = new UserProfile();

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Olá, digite seu nome.") };

            // Ask the user to enter their name.
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set the user's name to what they entered in response to the name prompt.
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Name = (string)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text(string.Format("Olá {0}, digite sua idade.", userProfile.Name)) };

            // Ask the user to enter their age.
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> StartSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set the user's age to what they entered in response to the age prompt.
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Age = (int)stepContext.Result;

            if (userProfile.Age < 18)
            {
                // If they are too young, skip the review selection dialog, and pass an empty list to the next step.
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("Você deve ter mais de 18 anos para participar."),
                    cancellationToken);
                return await stepContext.EndDialogAsync(stepContext.Values[UserInfo], cancellationToken);
            }
            else
            {
                // Otherwise, start the review selection dialog.
                return await stepContext.BeginDialogAsync(nameof(TimeSelectionDialog), null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AcknowledgementStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set the user's company selection to what they entered in the review-selection dialog.
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.CompaniesToReview = stepContext.Result as List<string> ?? new List<string>();

            // Thank them for participating.
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Obrigado por participar da nossa pesquisa, {((UserProfile)stepContext.Values[UserInfo]).Name}."),
                cancellationToken);

            // Exit the dialog, returning the collected user information.
            return await stepContext.EndDialogAsync(stepContext.Values[UserInfo], cancellationToken);
        }
    }
}
