// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using TwilioWhatsAppBot.Factories;
using TwilioWhatsAppBot.Models;

namespace TwilioWhatsAppBot.Dialogs
{
    public class LoopDialog : ComponentDialog
    {
        private List<Question> _questions = QuisFactory.GetQuestions();

        public LoopDialog()
            : base(nameof(LoopDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    QuestionStepAsync,
                    LoopStepAsync,
                }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> QuestionStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //Verifica se tem a questão atual informada
            var questionId = stepContext.Options as int? ?? 1;
            var questao = _questions.FirstOrDefault(x => x.Id == questionId);

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

        private async Task<DialogTurnResult> LoopStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var currentQuestion = (int)stepContext.Values["CurrentQuestion"];

            var question = _questions.FirstOrDefault(x => x.Id == currentQuestion);
            var nextQuestion = question.NextQuestion;

            if (question.Options != null)
            {
                var result = (FoundChoice)stepContext.Result;
                var option = question.Options.FirstOrDefault(x => x.Text == result.Value);
                if (option != null)
                    nextQuestion = option.NextQuestion.HasValue ? option.NextQuestion.Value : nextQuestion;
            }

            return await stepContext.ReplaceDialogAsync(nameof(LoopDialog), nextQuestion, cancellationToken);
        }
    }
}
