// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client.Core.DependencyInjection.Services;
using TwilioWhatsAppBot.Models;

namespace TwilioWhatsAppBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };
        private readonly IQueueService _queueService;
        public NotifyController(IQueueService queueService)
        {
            _queueService = queueService;
        }

        [HttpPost("{id}")]
        public async Task PostAsync(string id, Question question)
        {
            var message = JsonConvert.SerializeObject(question, jsonSettings);

            // Aend ResumeConversation event, it will get posted back to us with a specific value, giving us 
            // the ability to process it and do the right thing.
            await _queueService.SendJsonAsync(message, exchangeName: "exchange.name", routingKey: "question.key");

        }

        
    }
}