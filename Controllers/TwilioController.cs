using TwilioWhatsAppBot.CustomAdapter.TwilioWhatsApp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using System.Threading;
using System.Threading.Tasks;

namespace TwilioWhatsAppBot.Controllers
{
    [Route("api/twilio")]
    [ApiController]
    public class TwilioController : ControllerBase
    {
        private readonly TwilioWhatsAppAdapter _adapter;
        private readonly IBot _bot;

        public TwilioController(TwilioWhatsAppAdapter adapter, IBot bot)
        {
            _adapter = adapter;
            _bot = bot;
        }

        [HttpPost]
        [HttpGet]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot, default);
        }
    }
}