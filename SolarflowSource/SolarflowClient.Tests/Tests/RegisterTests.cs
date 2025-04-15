using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using Xunit;
using SolarflowServer.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

public class ResgisterTests
{
    private readonly string baseUrl = "https://localhost:7206";
    private ApplicationDbContext _dbContext;

    [Fact]
    public void Register_User()
    {

        using (var driver = new ChromeDriver())
        {
            driver.Navigate().GoToUrl(baseUrl + "/Authentication/Register");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            driver.FindElement(By.Name("Fullname")).SendKeys("Test User");
            driver.FindElement(By.Name("Email")).SendKeys("newuser@test.com");
            driver.FindElement(By.Name("Password")).SendKeys("Test@1231!!!");
            driver.FindElement(By.Name("ConfirmPassword")).SendKeys("Test@1231!!!");

            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            wait.Until(d => d.Url == baseUrl + '/');


            Assert.Equal(baseUrl + '/', driver.Url);
        }
    }
}