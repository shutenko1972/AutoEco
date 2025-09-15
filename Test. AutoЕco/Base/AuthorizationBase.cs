using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace Autotests_ai_ecosystem.Base
{
    public abstract class AuthorizationBase : BaseTest
    {
        protected const string ValidLogin = "v_shutenko";
        protected const string ValidPassword = "8nEThznM";
        protected const string TestLoginUrl = "https://ai-ecosystem-test.janusww.com:9999/auth/login.html";
        protected const string ProdLoginUrl = "https://ai-ecosystem-test.janusww.com:9999/auth/login.html";
        protected const string TestChatGPTUrl = "https://ai-ecosystem-test.janusww.com:9999/request/model.html";
        protected const string ExpectedUserId = "b906170e-d802-4a11-b3a5-f22714f854ba";

        // Переопределяем свойство для управления headless режимом
        // "on" - без визуализации, "off" - с визуализацией

        protected override string HeadlessOption => "on"; 
        // protected override string HeadlessOption => "off"; 

        protected void PerformLogin(string? loginUrl = null)
        {
            string url = loginUrl ?? TestLoginUrl;
            Console.WriteLine($"Переход на страницу входа: {url}");
            Console.WriteLine($"Режим работы: {(HeadlessOption == "on" ? "headless (без визуализации)" : "обычный (с визуализацией)")}");

            Driver.Navigate().GoToUrl(url);

            Wait.Until(d => d.FindElement(By.Id("loginform-login")).Displayed);

            Console.WriteLine("Ввод логина...");
            Driver.FindElement(By.Id("loginform-login")).SendKeys(ValidLogin);

            Console.WriteLine("Ввод пароля...");
            Driver.FindElement(By.Id("loginform-password")).SendKeys(ValidPassword);

            Console.WriteLine("Нажатие кнопки входа...");
            Driver.FindElement(By.CssSelector(".icon-circle-right2")).Click();

            Wait.Until(d => d.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user"))
                .Any(e => e.Displayed));

            Console.WriteLine("Вход выполнен успешно");

            // Увеличиваем время ожидания при визуализации для лучшего наблюдения
            if (HeadlessOption == "off")
            {
                Thread.Sleep(3000); // Дольше ждем при визуализации
            }
            else
            {
                Thread.Sleep(1000); // Короткая пауза в headless режиме
            }
        }

        protected void OpenUserMenu()
        {
            Console.WriteLine("Поиск меню пользователя...");
            var userMenu = Driver.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user, [data-toggle='dropdown'], .user-name"))
                .FirstOrDefault(e => e.Displayed && e.Text.Contains("Vitaliy"));

            if (userMenu == null)
            {
                userMenu = Driver.FindElements(By.CssSelector(".dropdown-toggle, .user-menu, .dropdown-user"))
                    .FirstOrDefault(e => e.Displayed);
            }

            NUnit.Framework.Assert.That(userMenu, Is.Not.Null, "Меню пользователя 'Vitaliy Shutenko' не найдено");
            Console.WriteLine("Открытие меню пользователя...");
            userMenu.Click();

            // Увеличиваем время ожидания при визуализации
            if (HeadlessOption == "off")
            {
                Thread.Sleep(1500);
            }
            else
            {
                Thread.Sleep(500);
            }
        }

        protected IWebElement FindAccountSettingsButton()
        {
            Console.WriteLine("Поиск кнопки Account Settings...");
            var accountSettingsButton = Driver.FindElements(By.LinkText("Account settings"))
                .FirstOrDefault(e => e.Displayed);

            if (accountSettingsButton == null)
            {
                accountSettingsButton = Driver.FindElements(By.PartialLinkText("Account"))
                    .FirstOrDefault(e => e.Displayed && e.Text.ToLower().Contains("account"));
            }

            NUnit.Framework.Assert.That(accountSettingsButton, Is.Not.Null, "Кнопка 'Account Settings' не найдена в меню");
            NUnit.Framework.Assert.That(accountSettingsButton.Enabled, Is.True, "Кнопка 'Account Settings' не активна");

            return accountSettingsButton;
        }

        protected void EnterTextAndVerify(string text, string elementId = "textarea_request")
        {
            Console.WriteLine($"Ввод текста: {text}");
            var textArea = Driver.FindElement(By.Id(elementId));
            textArea.Click();
            textArea.Clear();
            textArea.SendKeys(text);

            string enteredText = textArea.GetAttribute("value") ?? string.Empty;
            NUnit.Framework.Assert.That(enteredText, Is.EqualTo(text), $"Текст не был введен корректно: ожидалось '{text}', получено '{enteredText}'");

            // Небольшая пауза при визуализации для наблюдения за вводом
            if (HeadlessOption == "off")
            {
                Thread.Sleep(1000);
            }
        }

        protected void WaitForResponse(int timeoutSeconds = 90)
        {
            Console.WriteLine("Ожидание ответа...");
            bool responseReceived = false;
            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds && !responseReceived)
            {
                try
                {
                    var responseElements = Driver.FindElements(By.CssSelector(
                        ".content, .response, .answer, .message, .coping, " +
                        "[class*='response'], [class*='answer'], [class*='message']"
                    ));

                    var responseElement = responseElements
                        .FirstOrDefault(e => e.Displayed &&
                            !string.IsNullOrWhiteSpace(e.Text) &&
                            e.Text.Length > 10 &&
                            !e.Text.Contains("Temperature:"));

                    if (responseElement != null)
                    {
                        responseReceived = true;
                        Console.WriteLine("Ответ получен");

                        // Пауза для наблюдения за ответом при визуализации
                        if (HeadlessOption == "off")
                        {
                            Thread.Sleep(2000);
                        }
                        break;
                    }

                    var loadingElements = Driver.FindElements(By.CssSelector(
                        ".ladda-spinner, .loading, .spinner, [class*='loading']"
                    ));

                    bool isLoading = loadingElements.Any(e => e.Displayed);
                    if (!isLoading)
                    {
                        responseReceived = true;
                        Console.WriteLine("Индикаторы загрузки исчезли");
                        break;
                    }

                    Thread.Sleep(2000);
                }
                catch
                {
                    Thread.Sleep(2000);
                }
            }

            if (!responseReceived)
            {
                throw new Exception($"Ответ не получен в течение {timeoutSeconds} секунд");
            }
        }

        protected void HandleCommonExceptions(Action testAction, string testName)
        {
            try
            {
                testAction();
                Console.WriteLine($"ТЕСТ ПРОЙДЕН: {testName}");

                // Пауза после успешного теста при визуализации
                if (HeadlessOption == "off")
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ТЕСТ НЕ ПРОЙДЕН: {testName} - {ex.Message}");
                Console.WriteLine($"Текущий URL: {Driver.Url}");
                TakeScreenshot(testName + "_error");
                throw;
            }
        }
    }
}