using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;

namespace Autotests_ai_ecosystem.Base
{
    [TestFixture]
    public abstract class BaseTest
    {
        protected IWebDriver Driver { get; private set; } = null!;
        protected WebDriverWait Wait { get; private set; } = null!;

        // Параметр для управления headless режимом
        protected virtual string HeadlessOption => "on"; // "on" - headless, "off" - с визуализацией

        [SetUp]
        public void Setup()
        {
            var chromeOptions = new ChromeOptions();

            // Настройка headless режима
            if (HeadlessOption == "on")
            {
                chromeOptions.AddArgument("--headless");
                chromeOptions.AddArgument("--disable-gpu");
                chromeOptions.AddArgument("--no-sandbox");
                chromeOptions.AddArgument("--window-size=1920,1080");
                Console.WriteLine("Запуск в headless режиме (без визуализации)");
            }
            else
            {
                chromeOptions.AddArgument("--start-maximized");
                Console.WriteLine("Запуск в обычном режиме (с визуализацией)");
            }

            chromeOptions.AddArgument("--disable-notifications");
            chromeOptions.AddArgument("--disable-extensions");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddArgument("--disable-web-security");
            chromeOptions.AddArgument("--allow-running-insecure-content");

            Driver = new ChromeDriver(chromeOptions);
            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
                {
                    TakeScreenshot(TestContext.CurrentContext.Test.Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании скриншота: {ex.Message}");
            }
            finally
            {
                Driver?.Quit();
                Driver?.Dispose();
            }
        }

        protected void TakeScreenshot(string testName)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                var fileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var screenshotsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Screenshots");
                var screenshotPath = Path.Combine(screenshotsDirectory, fileName);

                Directory.CreateDirectory(screenshotsDirectory);
                screenshot.SaveAsFile(screenshotPath);

                TestContext.AddTestAttachment(screenshotPath);
                Console.WriteLine($"Скриншот сохранен: {screenshotPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось сделать скриншот: {ex.Message}");
            }
        }

        protected static void MarkTestAsFailed()
        {
            Console.WriteLine($"ТЕСТ ПРОВАЛЕН: {TestContext.CurrentContext.Test.Name}");
        }

        // Метод для принудительного изменения headless режима из тестов
        protected void SetHeadlessMode(string mode)
        {
            // Этот метод может быть использован для динамического изменения режима
            // Но обычно режим устанавливается один раз при запуске
            Console.WriteLine($"Запрошено изменение headless режима на: {mode}");
            Console.WriteLine("Внимание: для применения изменений требуется перезапуск драйвера");
        }
    }
}