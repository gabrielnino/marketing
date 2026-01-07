using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;
using Services;

namespace Marketing.Services.Test
{
    [TestFixture]
    public sealed class CaptureSnapshotTests
    {
        private Mock<IWebDriver> _driver = null!;
        private Mock<ILogger<CaptureSnapshot>> _logger = null!;

        [SetUp]
        public void SetUp()
        {
            _driver = new Mock<IWebDriver>(MockBehavior.Strict);
            _logger = new Mock<ILogger<CaptureSnapshot>>(MockBehavior.Loose);
        }

        [TearDown]
        public void TearDown()
        {
            _driver.VerifyNoOtherCalls();
            // ILogger is Loose; we verify specific messages in tests that need it.
        }

        [Test]
        public async Task Given_ValidExecutionFolderAndStage_When_CaptureArtifactsAsync_Then_ReturnsTimestampAndWritesHtmlAndPng()
        {
            // Given
            var executionFolder = CreateUniqueTempFolderPath();
            var stage = "BeforeSend";
            var pageSource = "<html><body>ok</body></html>";

            _driver.Setup(d => d.PageSource).Returns(pageSource);

            var takesScreenshot = _driver.As<ITakesScreenshot>();
            takesScreenshot.Setup(t => t.GetScreenshot())
                .Returns(CreateMinimalPngScreenshot());

            var sut = new CaptureSnapshot(_driver.Object, _logger.Object);

            // When
            var timestamp = await sut.CaptureArtifactsAsync(executionFolder, stage);

            // Then (observable contract)
            Assert.That(timestamp, Is.Not.Null.And.Not.Empty);
            Assert.That(IsTimestampFormat(timestamp), Is.True, "Timestamp must be yyyyMMdd_HHmmss.");

            var htmlPath = Path.Combine(executionFolder, $"{timestamp}.html");
            var pngPath = Path.Combine(executionFolder, $"{timestamp}.png");

            Assert.That(Directory.Exists(executionFolder), Is.True, "Execution folder must be created.");
            Assert.That(File.Exists(htmlPath), Is.True, "HTML artifact must be created.");
            Assert.That(File.Exists(pngPath), Is.True, "PNG artifact must be created.");

            var html = await File.ReadAllTextAsync(htmlPath);
            Assert.That(html, Is.EqualTo(pageSource), "HTML file must contain driver.PageSource.");

            // Dependency interactions (still contract-level)
            _driver.VerifyGet(d => d.PageSource, Times.Once);
            takesScreenshot.Verify(t => t.GetScreenshot(), Times.Once);
        }

        [Test]
        public async Task Given_StageIsNullOrWhitespace_When_CaptureArtifactsAsync_Then_UsesUnknownStageInLoggingAndStillCreatesArtifacts()
        {
            // Given
            var executionFolder = CreateUniqueTempFolderPath();
            string stage = "   ";
            var pageSource = "<html>stage-default</html>";

            _driver.Setup(d => d.PageSource).Returns(pageSource);

            var takesScreenshot = _driver.As<ITakesScreenshot>();
            takesScreenshot.Setup(t => t.GetScreenshot())
                .Returns(CreateMinimalPngScreenshot());

            var sut = new CaptureSnapshot(_driver.Object, _logger.Object);

            // When
            var timestamp = await sut.CaptureArtifactsAsync(executionFolder, stage);

            // Then
            var htmlPath = Path.Combine(executionFolder, $"{timestamp}.html");
            var pngPath = Path.Combine(executionFolder, $"{timestamp}.png");

            Assert.That(File.Exists(htmlPath), Is.True);
            Assert.That(File.Exists(pngPath), Is.True);

            // Verify it logged "UnknownStage" for stage (observable in logs)
            _logger.VerifyLogContains(
                Microsoft.Extensions.Logging.LogLevel.Information,
                "Capturing artifacts for stage",
                Times.AtLeastOnce(),
                mustContain: "UnknownStage");

            _driver.VerifyGet(d => d.PageSource, Times.Once);
            takesScreenshot.Verify(t => t.GetScreenshot(), Times.Once);
        }

        [Test]
        public void Given_ExecutionFolderIsNull_When_CaptureArtifactsAsync_Then_ThrowsArgumentNullException()
        {
            // Given
            var sut = new CaptureSnapshot(_driver.Object, _logger.Object);

            // When / Then
            Assert.ThrowsAsync<ArgumentNullException>(() => sut.CaptureArtifactsAsync(null!, "AnyStage"));
        }

        [Test]
        public void Given_DriverDoesNotImplementITakesScreenshot_When_CaptureArtifactsAsync_Then_ThrowsInvalidCastException()
        {
            // Given
            var executionFolder = CreateUniqueTempFolderPath();
            _driver.Setup(d => d.PageSource).Returns("<html/>");
            // IMPORTANT: do NOT configure _driver.As<ITakesScreenshot>() here, so cast fails.
            var sut = new CaptureSnapshot(_driver.Object, _logger.Object);

            // When / Then
            Assert.ThrowsAsync<InvalidCastException>(() => sut.CaptureArtifactsAsync(executionFolder, "Stage"));
            _driver.VerifyGet(d => d.PageSource, Times.Once);
        }

        [Test]
        public void Given_ExecutionFolderPointsToInvalidPath_When_CaptureArtifactsAsync_Then_Throws()
        {
            // Given
            // This is intentionally invalid on Windows due to '*'
            var executionFolder = Path.Combine(Path.GetTempPath(), "bad*folder");
            _driver.Setup(d => d.PageSource).Returns("<html/>");

            var takesScreenshot = _driver.As<ITakesScreenshot>();
            takesScreenshot.Setup(t => t.GetScreenshot())
                .Returns(CreateMinimalPngScreenshot());

            var sut = new CaptureSnapshot(_driver.Object, _logger.Object);

            // When / Then (Directory.CreateDirectory or Path operations should throw)
            Assert.ThrowsAsync<IOException>(() => sut.CaptureArtifactsAsync(executionFolder, "Stage"));
        }

        // ---------------------------
        // Helpers
        // ---------------------------

        private static string CreateUniqueTempFolderPath()
        {
            var root = Path.Combine(Path.GetTempPath(), "CaptureSnapshotTests");
            return Path.Combine(root, Guid.NewGuid().ToString("N"));
        }

        private static bool IsTimestampFormat(string timestamp)
        {
            // yyyyMMdd_HHmmss => 8 digits, underscore, 6 digits
            return Regex.IsMatch(timestamp, @"^\d{8}_\d{6}$");
        }

        private static Screenshot CreateMinimalPngScreenshot()
        {
            // Minimal valid 1x1 PNG (base64). This avoids flakiness and produces a real file.
            const string base64Png =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO3Z0XkAAAAASUVORK5CYII=";
            return new Screenshot(base64Png);
        }
    }

    internal static class LoggerMoqExtensions
    {
        public static void VerifyLogContains<T>(
            this Mock<ILogger<T>> logger,
            Microsoft.Extensions.Logging.LogLevel level,
            string contains,
            Times times,
            string? mustContain = null)
        {
            logger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state != null &&
                        state.ToString()!.Contains(contains, StringComparison.OrdinalIgnoreCase) &&
                        (mustContain == null ||
                         state.ToString()!.Contains(mustContain, StringComparison.OrdinalIgnoreCase))
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                times
            );
        }
    }

}
