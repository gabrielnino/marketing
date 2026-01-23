using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions.UrlValidation
{
    public interface IUrlAvailabilityValidatorFactory
    {
        IUrValidator GetValidator(string url);
    }
}
