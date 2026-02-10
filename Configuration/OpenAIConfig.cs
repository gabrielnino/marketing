using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public sealed class OpenAIConfig
    {
        public string ApiKey { get; init; } = null!;
        public string UriString { get; init; } = null!;
        public string Model { get; init; } = null!;
    }
}
