// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace TwilioWhatsAppBot.CustomAdapter.TwilioWhatsApp
{
    /// <summary>
    /// Options for the <see cref="TwilioAdapter"/>.
    /// </summary>
    public class TwilioAdapterOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether incoming requests should be validated as coming from Twilio.
        /// </summary>
        /// <value>
        /// A value indicating whether incoming requests should be validated as coming from Twilio.
        /// </value>
        public bool ValidateIncomingRequests { get; set; } = true;
    }
}
