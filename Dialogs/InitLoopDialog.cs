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
    public class InitLoopDialog : ComponentDialog
    {
        private List<Question> _questions = QuisFactory.GetQuestions();

        public InitLoopDialog()
            : base(nameof(LoopDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                QuestionStepAsync,
                LongOperationStepAsync,
                LoopStepAsync,
            };


            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitQuestionStepAsync
            }));

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new LongOperationPrompt(nameof(LongOperationPrompt), (vContext, token) =>
            {
                return Task.FromResult(vContext.Recognized.Succeeded);
            }));
        
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);

        }

        private async Task<DialogTurnResult> InitQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text($"Seja bem vindo ao nosso CHAT, já vamos te atender."),
                RetryPrompt = MessageFactory.Text("Tente novamente"),
            };

            // Start over
            return await stepContext.PromptAsync(nameof(LongOperationPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> QuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Verifica se tem a questão atual informada
            var questao = (Question)((Activity)stepContext.Result).Value;


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

        private async Task<DialogTurnResult> LoopStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

        private static async Task<DialogTurnResult> OperationTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the user's response is received.
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select a long operation test option."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "option 1", "option 2", "option 3" }),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> LongOperationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var prompt = MessageFactory.Text("...one moment please....");
            // The reprompt will be shown if the user messages the bot while the long operation is being performed.
            var retryPrompt = MessageFactory.Text($"Still performing the long operation...");
            return await stepContext.PromptAsync(nameof(LongOperationPrompt),
                                                        new LongOperationPromptOptions
                                                        {
                                                            Prompt = prompt,
                                                            RetryPrompt = retryPrompt
                                                        }, cancellationToken);
        }

        private static async Task<DialogTurnResult> OperationCompleteStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["longOperationResult"] = stepContext.Result;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks for waiting. { (stepContext.Result as Activity).Value}"), cancellationToken);

            // Start over
            return await stepContext.ReplaceDialogAsync(nameof(WaterfallDialog), null, cancellationToken);
        }
    }
}
