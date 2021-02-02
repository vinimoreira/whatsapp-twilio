// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using TwilioWhatsAppBot.CustomAdapter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TwilioWhatsAppBot.CustomAdapter.TwilioWhatsApp;
using TwilioWhatsAppBot.Bots;
using System.Collections.Concurrent;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client.Core.DependencyInjection;
using TwilioWhatsAppBot.Queue;

namespace TwilioWhatsAppBot
{
    public class Startup
    {
        public static IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the Twilio Adapter
            services.AddSingleton<TwilioWhatsAppAdapter, TwilioWhatsAppAdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create a global hashset for our ConversationReferences
            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            services.AddTransient<IBot, MainBot>();

            //Service used to queue into Queues
            services.AddSingleton<BotQueueService>();

            var rabbitMqSection = Configuration.GetSection("RabbitMq");
            var exchangeSection = Configuration.GetSection("RabbitMqExchange");
            var exchangeSectionQuestion = Configuration.GetSection("RabbitMqExchangeQuestion");

            services.AddRabbitMqClient(rabbitMqSection)
                .AddExchange("exchange.name", isConsuming: false, exchangeSection)
                .AddExchange("question.name", isConsuming: true, exchangeSectionQuestion)
                .AddMessageHandlerTransient<CustomMessageHandler>("question.key", exchange: "question.name");

            services.AddSingleton<IHostedService, ConsumingService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
