// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace TwilioWhatsAppBot.Dialogs
{
    public class TimeSelectionDialog : ComponentDialog
    {
        // Define a "done" response for the company selection prompt.
        private const string DoneOption = "done";

        // Define value names for values tracked inside the dialogs.
        private const string CompaniesSelected = "value-companiesSelected";

        // Define the company choices for the company selection prompt.
        private readonly string[] _companyOptions = new string[]
        {
            "Palmeiras", "Corinthians", "São Paulo", "Santos"
        };

        public TimeSelectionDialog()
            : base(nameof(TimeSelectionDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    SelectionStepAsync,
                    LoopStepAsync
                }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectionStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            // Continue using the same selection list, if any, from the previous iteration of this dialog.
            var list = stepContext.Options as List<string> ?? new List<string>();
            stepContext.Values[CompaniesSelected] = list;

            // Create a prompt message.
            string message;
            message = $"Por favor, escolha seu time ou digite `{DoneOption}` para encerrar.";

            // Create the list of options to choose from.
            var options = _companyOptions.ToList();
            options.Add(DoneOption);

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(message),
                RetryPrompt = MessageFactory.Text("Por favor escolha uma opção da lista"),
                Choices = ChoiceFactory.ToChoices(options),
            };

            // Prompt the user for a choice.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> LoopStepAsync(
           WaterfallStepContext stepContext,
           CancellationToken cancellationToken)
        {
            // Retrieve their selection list, the choice they made, and whether they chose to finish.
            var list = stepContext.Values[CompaniesSelected] as List<string>;
            var choice = (FoundChoice)stepContext.Result;
            var done = choice.Value == DoneOption;

            if (done)
            {
                // If they're done, exit and return their list.
                return await stepContext.EndDialogAsync(list, cancellationToken);
            }


            if (choice.Value != string.Empty)
            {
                list.Add(choice.Value);
                // If they're done, exit and return their list.
                return await stepContext.EndDialogAsync(list, cancellationToken);

            }

            // Otherwise, repeat this dialog, passing in the list from this iteration.
            return await stepContext.ReplaceDialogAsync(nameof(ReviewSelectionDialog), list, cancellationToken);


        }

    }
}
