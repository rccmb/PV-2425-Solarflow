using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using Xunit;
using SolarflowServer.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

public class LoginTest
{
    private readonly string baseUrl = "https://localhost:7206";
    private ApplicationDbContext _dbContext;

    [Fact]
    public void Login_User()
    {

        using (var driver = new ChromeDriver())
        {
            driver.Navigate().GoToUrl(baseUrl + "/Authentication/Register");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            driver.Navigate().GoToUrl(baseUrl);

            driver.FindElement(By.Name("Email")).SendKeys("newuser@test.com");
            driver.FindElement(By.Name("Password")).SendKeys("Test@1231!!!");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            wait.Until(d => d.Url.Contains("/Home/Index"));

            Assert.Contains("/Home/Index", driver.Url);
        }
    }
}