using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NavigationTests
{
    private readonly string baseUrl = "https://localhost:7206";

    [Fact]
    public void Login_Navigate_Logout()
    {
        using (var driver = new ChromeDriver())
        {
            driver.Navigate().GoToUrl(baseUrl);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            driver.FindElement(By.Name("Email")).SendKeys("newuser@test.com");
            driver.FindElement(By.Name("Password")).SendKeys("Test@1231!!!");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Url.Contains("/Home/Index"));
            Assert.Contains("/Home/Index", driver.Url);

            // Navigate through menu items
            string[] menuItems = { "Home", "Battery", "Suggestions", "Notifications", "Settings" };
            foreach (string item in menuItems)
            {
                IWebElement menuItem = driver.FindElement(By.XPath($"//a[contains(@href, '/{item}/Index')]"));
                menuItem.Click();
                wait.Until(d => d.Url.Contains($"/{item}/Index"));
                Assert.Contains($"/{item}/Index", driver.Url);
            }

            // Logout
            IWebElement logoutButton = driver.FindElement(By.CssSelector(".logout-section button"));
            logoutButton.Click();
            wait.Until(d => d.Url.Contains(baseUrl));
            Assert.Contains(baseUrl, driver.Url);
        }
    }
}

