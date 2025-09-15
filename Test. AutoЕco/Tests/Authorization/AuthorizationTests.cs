using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;
using Autotests_ai_ecosystem.Base;

namespace Autotests_ai_ecosystem.Tests.Authorization
{
    [TestFixture]
    public class AuthorizationTests : Base.AuthorizationBase
    {
        [Test]
        public void AuthorizationLogInTest()
        {
            HandleCommonExceptions(() =>
            {
                PerformLogin();
                Assert.That(Driver.Url.Contains("login") || Driver.Url.Contains("auth/login"), Is.False,
                    "Остались на странице входа после успешного логина");
            }, "AuthorizationLogInTest");
        }

        [Test]
        public void AuthorizationLogInLogoutTest()
        {
            HandleCommonExceptions(() =>
            {
                PerformLogin();
                Thread.Sleep(2000);

                OpenUserMenu();
                Thread.Sleep(1000);

                var logoutButton = Driver.FindElements(By.LinkText("Logout"))
                    .FirstOrDefault(e => e.Displayed);

                if (logoutButton == null)
                {
                    logoutButton = Driver.FindElements(By.PartialLinkText("Logout"))
                        .FirstOrDefault(e => e.Displayed);
                }

                Assert.That(logoutButton, Is.Not.Null, "Кнопка выхода не найдена");
                logoutButton.Click();

                Wait.Until(d => d.FindElement(By.Id("loginform-login")).Displayed);
                Assert.That(Driver.FindElement(By.Id("loginform-login")).Displayed, Is.True,
                    "Не вернулись на страницу входа после выхода");

            }, "AuthorizationLogInLogoutTest");
        }
    }
}