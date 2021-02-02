using TwilioWhatsAppBot.CustomAdapter.TwilioWhatsApp;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TwilioWhatsAppBot.CustomAdapter
{
    public class TwilioWhatsAppAdapterWithErrorHandler : TwilioWhatsAppAdapter
    {
        public TwilioWhatsAppAdapterWithErrorHandler(IConfiguration configuration, ILogger<TwilioWhatsAppAdapter> logger)
                : base(configuration, null, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                // Send a message to the user
                await turnContext.SendActivityAsync("Não foi possível processar a requisição.");

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
