using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Threading;
using Autotests_ai_ecosystem.Base;

namespace Autotests_ai_ecosystem.Tests.ChatGPT
{
    [TestFixture]
    public class ChatGPTTests : Base.AuthorizationBase
    {
        [Test]
        public void RequestChatGPTClearInput()
        {
            HandleCommonExceptions(() =>
            {
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);

                Wait.Until(d => d.FindElement(By.Id("textarea_request")).Displayed);
                EnterTextAndVerify("Привет!");

                var clearButton = Driver.FindElement(By.Id("clear_request"));
                clearButton.Click();

                Wait.Until(d => string.IsNullOrEmpty(d.FindElement(By.Id("textarea_request")).GetAttribute("value")));
                string clearedText = Driver.FindElement(By.Id("textarea_request")).GetAttribute("value") ?? string.Empty;

                Assert.That(string.IsNullOrEmpty(clearedText), Is.True,
                    $"Поле ввода не было очищено. Текущее содержимое: '{clearedText}'");

            }, "RequestChatGPTClearInput");
        }

        [Test]
        public void RequestChatGPTSendRequest()
        {
            HandleCommonExceptions(() =>
            {
                PerformLogin();
                Driver.Navigate().GoToUrl(TestChatGPTUrl);

                Wait.Until(d => d.FindElement(By.Id("textarea_request")).Displayed);
                EnterTextAndVerify("Привет!");

                var sendButton = Driver.FindElement(By.CssSelector(".ladda-label"));
                sendButton.Click();

                WaitForResponse();

            }, "RequestChatGPTSendRequest");
        }

        [Test]
        public void RequestChatGPTCopyAnswer()
        {
            HandleCommonExceptions(() =>
            {
                PerformLogin();
                NavigateToChatGPT();

                EnterTextAndVerify("Ghbdtn! Lhepmz!");

                var sendButton = Driver.FindElement(By.CssSelector(".ladda-label"));
                sendButton.Click();

                WaitForResponse();

                var copyButton = Driver.FindElement(By.CssSelector(".coping"));
                Assert.That(copyButton.Displayed, Is.True, "Кнопка копирования не отображается");
                copyButton.Click();

                Thread.Sleep(1000); // Даем время для копирования

                var textArea = Driver.FindElement(By.Id("textarea_request"));
                textArea.Click();
                textArea.Clear();

                string safeText = "Привет! Чем могу помочь?";
                textArea.SendKeys(safeText);

                // Проверяем, что новый текст введен
                string newText = textArea.GetAttribute("value") ?? string.Empty;
                Assert.That(newText, Is.EqualTo(safeText), "Новый текст не был введен корректно");

                TakeScreenshot("копирование_успешно");

            }, "RequestChatGPTCopyAnswer");
        }

        private void NavigateToChatGPT()
        {
            Driver.Navigate().GoToUrl(TestChatGPTUrl);
            Wait.Until(d => d.FindElement(By.Id("textarea_request")).Displayed);
        }

        private void EnterTextAndVerify(string text)
        {
            var textArea = Driver.FindElement(By.Id("textarea_request"));
            textArea.Clear();
            textArea.SendKeys(text);

            string enteredText = textArea.GetAttribute("value") ?? string.Empty;
            Assert.That(enteredText, Is.EqualTo(text), $"Текст '{text}' не был введен корректно. Получено: '{enteredText}'");
        }

        private void WaitForResponse()
        {
            Wait.Until(d => d.FindElement(By.CssSelector(".coping")).Displayed);
        }
    }
}