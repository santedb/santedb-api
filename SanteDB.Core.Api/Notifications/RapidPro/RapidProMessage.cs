using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SanteDB.Core.Notifications.RapidPro
{
    public class RapidProMessage
    {
        [JsonProperty("contact")]
        public string Contact { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
