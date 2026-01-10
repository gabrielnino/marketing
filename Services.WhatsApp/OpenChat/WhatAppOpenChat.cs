using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.WhatsApp.Abstractions.Login;
using Services.WhatsApp.Abstractions.OpenChat;
using Services.WhatsApp.Abstractions.Search;
using Services.WhatsApp.Abstractions.XPath;
using Services.WhatsApp.Login;
using Services.WhatsApp.Search;
using Services.WhatsApp.Selector;
using Services.WhatsApp.Selenium;
using Services.WhatsApp.XPath;

namespace Services.WhatsApp.OpenChat
{

    public class WhatAppOpenChat : IWhatAppOpenChat
    {
        private const string WhatAppMessage = "WhatsApp Web is not logged in. Call LoginAsync() before opening a chat.";

        // Keep original logger type/category to avoid changing logging category semantics.
        public ILogger<LoginService> Logger { get; }

        private readonly IWhatsAppLoginStateChecker _loginChecker;
        private readonly IWhatsAppSearchBoxTyper _searchTyper;
        private readonly IWhatsAppChatClicker _chatClicker;

        // Backward-compatible constructor (same signature as original).
        public WhatAppOpenChat(IWebDriver driver, ILogger<LoginService> logger)
            : this(
                loginChecker: new WhatsAppLoginStateChecker(
                    driver: new SeleniumWebDriverFacade(driver),
                    selectors: new WhatsAppSelectors(),
                    logger: logger),
                searchTyper: new WhatsAppSearchBoxTyper(
                    driver: new SeleniumWebDriverFacade(driver),
                    selectors: new WhatsAppSelectors(),
                    logger: logger),
                chatClicker: new WhatsAppChatClicker(
                    driver: new SeleniumWebDriverFacade(driver),
                    selectors: new WhatsAppSelectors(),
                    xpathBuilder: new ChatXPathBuilder(new XPathLiteralEscaper(logger)),
                    logger: logger),
                logger: logger)
        {
        }

        // Injectable constructor for testing/DI (DIP).
        public WhatAppOpenChat(
            IWhatsAppLoginStateChecker loginChecker,
            IWhatsAppSearchBoxTyper searchTyper,
            IWhatsAppChatClicker chatClicker,
            ILogger<LoginService> logger)
        {
            _loginChecker = loginChecker ?? throw new ArgumentNullException(nameof(loginChecker));
            _searchTyper = searchTyper ?? throw new ArgumentNullException(nameof(searchTyper));
            _chatClicker = chatClicker ?? throw new ArgumentNullException(nameof(chatClicker));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task OpenContactChatAsync(
            string chatIdentifier,
            TimeSpan? timeout = null,
            TimeSpan? pollInterval = null,
            CancellationToken ct = default)
        {
            Logger.LogInformation("OpenContactChatAsync started. chatIdentifier='{ChatIdentifier}'", chatIdentifier);

            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(chatIdentifier))
            {
                Logger.LogWarning("OpenContactChatAsync aborted: chatIdentifier is null/empty/whitespace.");
                throw new ArgumentException("Chat identifier cannot be null, empty, or whitespace.", nameof(chatIdentifier));
            }

            // 1) Logged-in check
            Logger.LogInformation("Step 1/4: Checking WhatsApp Web login state...");
            if (!_loginChecker.IsWhatsAppLoggedIn())
            {
                Logger.LogError("OpenContactChatAsync failed: WhatsApp Web is not logged in.");
                throw new InvalidOperationException(WhatAppMessage);
            }
            Logger.LogInformation("Step 1/4: Logged in confirmed.");

            // 2) Resolve timeouts (kept verbatim)
            var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(10);
            var effectivePoll = pollInterval ?? TimeSpan.FromMilliseconds(200);

            if (effectiveTimeout <= TimeSpan.Zero)
            {
                Logger.LogWarning("Invalid timeout provided: {Timeout}.", effectiveTimeout);
                throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
            }

            if (effectivePoll <= TimeSpan.Zero)
            {
                Logger.LogWarning("Invalid pollInterval provided: {PollInterval}.", effectivePoll);
                throw new ArgumentOutOfRangeException(nameof(pollInterval), "Poll interval must be greater than zero.");
            }

            if (effectivePoll > effectiveTimeout)
            {
                var adjusted = TimeSpan.FromMilliseconds(Math.Max(50, effectiveTimeout.TotalMilliseconds / 10));
                Logger.LogWarning(
                    "PollInterval {PollInterval} > Timeout {Timeout}. Adjusting pollInterval to {AdjustedPollInterval}.",
                    effectivePoll, effectiveTimeout, adjusted);

                effectivePoll = adjusted;
            }

            Logger.LogInformation(
                "Step 2/4: Using timeout={Timeout} pollInterval={PollInterval}.",
                effectiveTimeout, effectivePoll);

            ct.ThrowIfCancellationRequested();

            // 3) Type into search box
            Logger.LogInformation("Step 3/4: Typing chatIdentifier into WhatsApp search box...");
            try
            {
                await _searchTyper.TypeIntoSearchBoxAsync(chatIdentifier, effectiveTimeout, effectivePoll /*, ct */)
                    .ConfigureAwait(false);

                Logger.LogInformation("Step 3/4: Search input completed.");
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("OpenContactChatAsync canceled during Step 3/4 (TypeIntoSearchBoxAsync).");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "OpenContactChatAsync failed during Step 3/4 (TypeIntoSearchBoxAsync).");
                throw;
            }

            ct.ThrowIfCancellationRequested();

            // 4) Click chat by title
            Logger.LogInformation("Step 4/4: Clicking chat by title '{ChatIdentifier}'...", chatIdentifier);
            try
            {
                await _chatClicker.ClickChatByTitleAsync(chatIdentifier, effectiveTimeout, effectivePoll /*, ct */)
                    .ConfigureAwait(false);

                Logger.LogInformation("Step 4/4: Chat opened successfully. chatIdentifier='{ChatIdentifier}'", chatIdentifier);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("OpenContactChatAsync canceled during Step 4/4 (ClickChatByTitleAsync).");
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "OpenContactChatAsync failed during Step 4/4 (ClickChatByTitleAsync).");
                throw;
            }
        }
    }

}
