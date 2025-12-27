using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Messages
{
    public sealed class ImageMessagePayload
    {
        public string StoredImagePath { get; init; } = default!;
        public string Caption { get; init; } = string.Empty;
    }
}
