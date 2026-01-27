using Application.PixVerse.Response;
using Application.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.PixVerse
{
    public interface IBalanceClient
    {
        Task<Operation<Balance>> GetAsync(CancellationToken ct = default);
    }
}
