﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
    public class LoopDialog : ComponentDialog
    {
        private List<Question> _questions = QuisFactory.GetQuestions();

        public LoopDialog()
            : base(nameof(LoopDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                InitQuestionStepAsync,
                QuestionStepAsync,
                LongOperationStepAsync,
                LoopStepAsync,
            };

            var waterfallSteps2 = new WaterfallStep[]
           {
                QuestionStepAsync,
                LongOperationStepAsync,
                LoopStepAsync,
           };


            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new WaterfallDialog("LoopDialog", waterfallSteps2));


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
            var questao = (Question)stepContext.Options ?? (Question)((Activity)stepContext.Result).Value ;

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

        private async Task<DialogTurnResult> LoopStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var currentQuestion = (Question)((Activity)stepContext.Result).Value;
            return await stepContext.ReplaceDialogAsync("LoopDialog", currentQuestion, cancellationToken);
        }

        private static async Task<DialogTurnResult> LongOperationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var prompt = MessageFactory.Text("Por favor aguarde....");
            // The reprompt will be shown if the user messages the bot while the long operation is being performed.
            var retryPrompt = MessageFactory.Text($"Ainda estamos processando sua operacao...");
            return await stepContext.PromptAsync(nameof(LongOperationPrompt),
                                                        new LongOperationPromptOptions
                                                        {
                                                            Prompt = null,
                                                            RetryPrompt = retryPrompt
                                                        }, cancellationToken);
        }

    }
}
