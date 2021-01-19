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
    public class LongOperationPromptOptions : PromptOptions
    {
        /// <summary>
        /// This is a property sent through the Queue, and is used
        /// in the queue consumer (the Azure Function) to differentiate 
        /// between long operations chosen by the user.
        /// </summary>
        public string LongOperationOption { get; set; }
    }
}
