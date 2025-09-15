using OpenQA.Selenium;
using System;

namespace Autotests_ai_ecosystem.Utilities
{
    public static class CommonHelpers
    {
        public static void TakeScreenshot(IWebDriver driver, string testName)
        {
            try
            {
                if (driver is ITakesScreenshot screenshotDriver)
                {
                    Screenshot screenshot = screenshotDriver.GetScreenshot();
                    string fileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    screenshot.SaveAsFile(fileName);
                    Console.WriteLine($"Скриншот сохранен: {fileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании скриншота: {ex.Message}");
            }
        }

        public static void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
        }

        public static void LogError(string message, Exception? ex = null)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}");
            if (ex != null)
            {
                Console.WriteLine($"Исключение: {ex.Message}");
            }
        }
    }
}