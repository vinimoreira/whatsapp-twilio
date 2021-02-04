using Microsoft.Extensions.Logging;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwilioWhatsAppBot.Utils
{
    public static class HtmlRenderer
    {
        public static string Render(string template, string templateKey, object model = null)
        {
            return Engine.Razor.RunCompile(template, templateKey, null, model);
        }
    }
}
