using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.TrackedLinks;

public interface ITrackedLink
{
    Task UpsertAsync(string id, string targetUrl, CancellationToken ct = default);
}
