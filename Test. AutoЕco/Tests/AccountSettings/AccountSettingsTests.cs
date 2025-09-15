using NUnit.Framework;
using OpenQA.Selenium;
using System;
using Autotests_ai_ecosystem.Base;

namespace Autotests_ai_ecosystem.Tests.AccountSettings

{
    [TestFixture]
    public class AccountSettingsTests : Base.AuthorizationBase
    {
        [Test]
        public void LoginVitaliyShutenkoAccountSettingsTest()
        {
            HandleCommonExceptions(() =>
            {
                PerformLogin(ProdLoginUrl);
                string urlBeforeClick = Driver.Url;

                OpenUserMenu();
                var accountSettingsButton = FindAccountSettingsButton();

                string currentUrl = Driver.Url;
                accountSettingsButton.Click();

                Wait.Until(d => d.Url != currentUrl);
                string urlAfterClick = Driver.Url;

                Assert.That(urlAfterClick, Is.Not.EqualTo(currentUrl),
                    "URL не изменился после нажатия кнопки Account Settings");

            }, "LoginVitaliyShutenkoAccountSettingsTest");
        }

        [Test]
        public void AccountsettingsCopytoclipboardTest()
        {
            HandleCommonExceptions(() =>
            {
                PerformLogin(ProdLoginUrl);
                OpenUserMenu();

                var accountSettingsButton = FindAccountSettingsButton();
                string currentUrl = Driver.Url;
                accountSettingsButton.Click();

                Wait.Until(d => d.Url != currentUrl);

                var copyButton = Driver.FindElement(By.CssSelector(".icon-clippy"));
                Assert.That(copyButton.Enabled, Is.True, "Кнопка 'Copy to clipboard' не активна");
                copyButton.Click();
                Thread.Sleep(1000);

                var homeButton = Driver.FindElement(By.LinkText("Home"));
                homeButton.Click();

                Wait.Until(d => d.Url.Contains("home") || d.FindElement(By.Id("textarea_request")).Displayed);

                var textArea = Driver.FindElement(By.Id("textarea_request"));
                textArea.Clear();
                textArea.SendKeys(Keys.Control + "v");

                string clipboardContent = textArea.GetAttribute("value") ?? string.Empty;
                Assert.That(clipboardContent, Is.EqualTo(ExpectedUserId),
                    $"Содержимое буфера обмена '{clipboardContent}' не совпадает с ожидаемым '{ExpectedUserId}'");

            }, "AccountsettingsCopytoclipboardTest");
        }
    }
}