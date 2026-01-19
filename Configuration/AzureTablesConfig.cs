using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configuration
{
    public sealed class AzureTablesConfig
    {
        public const string SectionName = "AzureTables";

        public string ServiceSasUrl { get; init; } = default!;
    }
}
