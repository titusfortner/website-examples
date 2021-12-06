using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace Selenium3
{
    [TestClass]
    public class ShadowRootNewChrome
    {
        private IWebDriver _driver;

        [TestCleanup]
        public void CleanUpAfterEveryTestMethod()
        {
            if (_driver == null) return;
            _driver.Quit();
        }

        private void StartSauceDriver(string browserVersion)
        {
            var username = Environment.GetEnvironmentVariable("SAUCE_USERNAME");
            var accessKey = Environment.GetEnvironmentVariable("SAUCE_ACCESS_KEY");

            var browserOptions = new ChromeOptions()
            {
                BrowserVersion = browserVersion,
                PlatformName = "Windows 10"
            };

            var addr = "https://" + username + ":" + accessKey + "@ondemand.saucelabs.com/wd/hub";
            
            _driver = new RemoteWebDriver(new Uri(addr), 
                browserOptions.ToCapabilities());
        }

        [TestMethod]
        public void ShadowDomPreviouslyWorkingChrome95()
        {
            StartSauceDriver("95.0");
            _driver.Navigate().GoToUrl("http://watir.com/examples/shadow_dom.html");
            var shadowHost = _driver.FindElement(By.CssSelector("#shadow_host"));
            var js = ((IJavaScriptExecutor)_driver);

            var shadowRoot = (IWebElement)js.ExecuteScript("return arguments[0].shadowRoot", shadowHost);
            IWebElement shadowContent = shadowRoot.FindElement(By.CssSelector("#shadow_content"));

            Assert.AreEqual("some text", shadowContent.Text);
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidCastException),"No Longer Works with IWebElement")]
        public void ShadowDomPreviouslyWorkingChrome96()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            _driver = new ChromeDriver();

            _driver.Navigate().GoToUrl("http://watir.com/examples/shadow_dom.html");
            var shadowHost = _driver.FindElement(By.CssSelector("#shadow_host"));
            var js = ((IJavaScriptExecutor)_driver);

            var shadowRoot = (IWebElement)js.ExecuteScript("return arguments[0].shadowRoot", shadowHost);
        }
        
        [TestMethod]
        public void ShadowDomFirefox()
        {
            new DriverManager().SetUpDriver(new FirefoxConfig());
            _driver = new FirefoxDriver();

            _driver.Navigate().GoToUrl("http://watir.com/examples/shadow_dom.html");
            var shadowHost = _driver.FindElement(By.CssSelector("#shadow_host"));
            var js = ((IJavaScriptExecutor)_driver);

            var children = (IEnumerable<IWebElement>)js.ExecuteScript("return arguments[0].shadowRoot.children", shadowHost);

            IWebElement shadowContent = null;
            
            foreach (IWebElement element in children) {
                if (element.GetAttribute("id").Equals("shadow_content")) {
                    shadowContent = element;
                    break;
                }
            }

            Assert.AreEqual("some text", shadowContent?.Text);
        }

        [TestMethod]
        public void ShadowRootNewChromeHacky()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            var options = new ChromeOptions();
            _driver = new ChromeDriver(options);

            _driver.Navigate().GoToUrl("http://watir.com/examples/shadow_dom.html");
            var shadowHost = _driver.FindElement(By.CssSelector("#shadow_host"));
            var js = ((IJavaScriptExecutor)_driver);

            var shadowRoot = (Dictionary<string, object>)js.ExecuteScript("return arguments[0].shadowRoot", shadowHost);
            var id = (string)shadowRoot["shadow-6066-11e4-a52e-4f735466cecf"];
            var shadowRootElement = new RemoteWebElement((RemoteWebDriver)_driver, id);

            var shadowContent = shadowRootElement.FindElement(By.CssSelector("#shadow_content"));
            var text = shadowContent.Text;

            Assert.AreEqual("some text", text);
        }
        
    }
}
