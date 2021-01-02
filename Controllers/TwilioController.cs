// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using TwilioWhatsAppBot.CustomAdapter.TwilioWhatsApp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace TwilioWhatsAppBot.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/twilio")]
    [ApiController]
    public class TwilioController : ControllerBase
    {
        private readonly TwilioWhatsAppAdapter _adapter;
        private readonly IBot _bot;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotController"/> class.
        /// </summary>
        /// <param name="adapter">adapter for the BotController.</param>
        /// <param name="bot">bot for the BotController.</param>
        public TwilioController(TwilioWhatsAppAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        /// <summary>
        /// PostAsync method that returns an async Task.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [HttpGet]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await _adapter.ProcessAsync(Request, Response, _bot, default(CancellationToken));
        }
    }
}