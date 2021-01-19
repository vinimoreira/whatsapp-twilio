// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using TwilioWhatsAppBot;
using TwilioWhatsAppBot.Models;

namespace TwilioWhatsAppBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly UserState _userState;

        public MainDialog(UserState userState)
            : base(nameof(MainDialog))
        {
            _userState = userState;

            AddDialog(new LoopDialog());

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                QuestionStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> QuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Verifica se tem a questão atual informada
            var questao = (Question)stepContext.Options ?? (Question)((Activity)stepContext.Result).Value;

            if (questao.End)
                return await stepContext.EndDialogAsync(null, cancellationToken);

            //Pega o texto da pergunta
            string message = questao.Text;

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(message),
                RetryPrompt = MessageFactory.Text("Tente novamente"),
            };

            string typeQuestion;
            switch (questao.Type)
            {
                case "options":
                    typeQuestion = nameof(ChoicePrompt);
                    promptOptions.Choices = ChoiceFactory.ToChoices(questao.Options.Select(x => x.Text).ToList());
                    break;
                case "number":
                    typeQuestion = nameof(NumberPrompt<int>);
                    break;
                default:
                    typeQuestion = nameof(TextPrompt);
                    break;
            }

            stepContext.Values.Add("CurrentQuestion", questao.Id);

            // Prompt the user for a choice.
            return await stepContext.PromptAsync(typeQuestion, promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(LoopDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userInfo = (UserProfile)stepContext.Result;

            if (userInfo.CompaniesToReview.Count > 0)
            {
                string status = string.Empty;

                if (userInfo.CompaniesToReview[0] == "Palmeiras")
                    status = "Parabéns, vc torce para o maior time do Brasil!";
                else
                    status = string.Format("Aff, você torce para o {0}. Quem tem mais tem 10", userInfo.CompaniesToReview[0]);

                await stepContext.Context.SendActivityAsync(status);
            }

            var accessor = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            await accessor.SetAsync(stepContext.Context, userInfo, cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
