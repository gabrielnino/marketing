using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace Services.WhatsApp.Abstractions.Search
{
    public interface IAttachments
    {
        IWebElement FindAttachButton(TimeSpan timeout, TimeSpan pollingInterval);
        IWebElement FindPhotosAndVideosOptionButton(TimeSpan timeout, TimeSpan pollingInterval);
    }
}
