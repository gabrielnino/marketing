using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.WhatsApp.OpenAI
{
    public class Prompt
    {
        public required string SystemContent { get; set; }
        public required string UserContent { get; set; }
    }
}
