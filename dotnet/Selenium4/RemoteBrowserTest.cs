using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace Selenium4
{
    [TestClass]
    public class RemoteBrowser
    {
        private IWebDriver _driver;

        private void StartSauceDriver(DriverOptions driverOptions)
        {
            var username = Environment.GetEnvironmentVariable("SAUCE_USERNAME");
            var accessKey = Environment.GetEnvironmentVariable("SAUCE_ACCESS_KEY");
            
            var address = "https://" + username + ":" + accessKey + "@ondemand.us-west-1.saucelabs.com/wd/hub";
            driverOptions.PlatformName = "Windows 10";
            
            driverOptions.AddAdditionalOption("sauce:options", new Dictionary<string, object>());
            
            _driver = new RemoteWebDriver(new Uri(address), driverOptions);
        }

        [TestCleanup]
        public void CleanUpAfterEveryTestMethod()
        {
            if (_driver == null) return;
            _driver.Quit();
        }

        // Chromium only
        [TestMethod]
        public void ChangeNetworkConditions()
        {
            StartSauceDriver(new ChromeOptions());
            
            var customCommandDriver = _driver as ICustomDriverCommandExecutor;
            customCommandDriver.RegisterCustomDriverCommands(ChromeDriver.CustomCommandDefinitions);

            var networkConditions = new ChromiumNetworkConditions { IsOffline = true };
            var offlineNetwork = new Dictionary<string, object> { { "network_conditions", networkConditions } };
            customCommandDriver.ExecuteCustomDriverCommand(ChromiumDriver.SetNetworkConditionsCommand, offlineNetwork);

            try
            {
                _driver.Navigate().GoToUrl("https://www.selenium.dev/");
            }
            catch (WebDriverException)
            {
                // Can not navigate when network conditions are set to offline
            }

            // reset network conditions
            customCommandDriver.ExecuteCustomDriverCommand(ChromiumDriver.DeleteNetworkConditionsCommand, null);

            networkConditions = new ChromiumNetworkConditions
            {
                Latency = TimeSpan.FromSeconds(1),
                DownloadThroughput = 50000,
                UploadThroughput = 50000
            };
            var limitedNetwork = new Dictionary<string, object> { { "network_conditions", networkConditions } };
            customCommandDriver.ExecuteCustomDriverCommand(ChromiumDriver.SetNetworkConditionsCommand, limitedNetwork);

            _driver.Navigate().GoToUrl("https://www.selenium.dev/");
        }

        // Firefox Only
        [TestMethod]
        public void ChangePreferences()
        {
            // Set default language to German
            var firefoxOptions = new FirefoxOptions();
            firefoxOptions.SetPreference("intl.accept_languages", "de-DE");

            StartSauceDriver(firefoxOptions);
            var js = (IJavaScriptExecutor) _driver;

            var customCommandDriver = _driver as ICustomDriverCommandExecutor;
            customCommandDriver.RegisterCustomDriverCommands(FirefoxDriver.CustomCommandDefinitions);

            _driver.Navigate().GoToUrl("https://www.google.com");
            
            // German content displayed
            var element = _driver.FindElement(By.CssSelector("#gws-output-pages-elements-homepage_additional_languages__als"));
            Assert.IsTrue(element.Text.Contains("angeboten auf"));

            try
            {
                js.ExecuteScript("Services.prefs.setStringPref('intl.accept_languages', 'es-ES')");
            }
            catch (WebDriverException)
            {
                // Can not change preferences in default "content" context
            }

            // Set context to "chrome"
            var chromePayload = new Dictionary<string, object> { { "context", "chrome" } };
            customCommandDriver.ExecuteCustomDriverCommand(FirefoxDriver.SetContextCommand, chromePayload);

            // Change Language Preference to Spanish
            js.ExecuteScript("Services.prefs.setStringPref('intl.accept_languages', 'es-ES')");

            try
            {
                _driver.Navigate().Refresh();
            }
            catch (WebDriverException)
            {
                // Can not navigate in "chrome" context
            }

            // Set context back to "content"
            var contentPayload = new Dictionary<string, object> { { "context", "content" } };
            customCommandDriver.ExecuteCustomDriverCommand(FirefoxDriver.SetContextCommand, contentPayload);

            // Navigation works in "content" context
            _driver.Navigate().Refresh();
            
            // Spanish content displayed
            element = _driver.FindElement(By.CssSelector("#gws-output-pages-elements-homepage_additional_languages__als"));
            Assert.IsTrue(element.Text.Contains("Ofrecido por"));
        }

        // Firefox Only
        [TestMethod]
        public void FullPageScreenshotOldWay()
        {
            StartSauceDriver(new FirefoxOptions());
            IHasCommandExecutor hasCommandExecutor = _driver as IHasCommandExecutor;

            const string resourcePath = "/session/{sessionId}/moz/screenshot/full";
            
            var addFullPageScreenshotCommandInfo = new HttpCommandInfo(
                HttpCommandInfo.GetCommand, 
                resourcePath);

            hasCommandExecutor.CommandExecutor.TryAddCommand(
                "fullPageScreenshot", 
                addFullPageScreenshotCommandInfo);

            const Dictionary<string, object> parameters = null;
            
            SessionId sessionId = ((RemoteWebDriver)_driver).SessionId;

            var fullPageScreenshotCommand = new Command(
                sessionId, 
                "fullPageScreenshot", 
                parameters);

            _driver.Navigate().GoToUrl("https://www.saucedemo.com/v1/inventory.html");

            var screenshotResponse = hasCommandExecutor
                .CommandExecutor
                .Execute(fullPageScreenshotCommand);
            
            SaveScreenshot(screenshotResponse.Value.ToString());
        }

        // Firefox Only
        [TestMethod]
        public void FullPageScreenshot()
        {
            StartSauceDriver(new FirefoxOptions());
            var customCommandDriver = _driver as ICustomDriverCommandExecutor;

            customCommandDriver.RegisterCustomDriverCommands(
                FirefoxDriver.CustomCommandDefinitions);

            _driver.Navigate().GoToUrl("https://www.selenium.dev/");

            const Dictionary<string, object> parameters = null;

            var screenshotResponse = customCommandDriver
                .ExecuteCustomDriverCommand(
                    FirefoxDriver.GetFullPageScreenshotCommand, 
                    parameters);

            SaveScreenshot((string) screenshotResponse);
        }
        
        private static void SaveScreenshot(String base64)
        {
            Screenshot image = new Screenshot(base64);
            var parentFullName = Directory
                .GetParent(Environment.CurrentDirectory)
                ?.Parent?.Parent?.FullName;
            image.SaveAsFile(
                parentFullName + "/Resources/FirefoxFullPageScreenshot.png", 
                ScreenshotImageFormat.Png);

        }

        // Firefox only
        [TestMethod]
        public void AddOns()
        {
            StartSauceDriver(new FirefoxOptions());
            
            var customCommandDriver = _driver as ICustomDriverCommandExecutor;
            customCommandDriver.RegisterCustomDriverCommands(FirefoxDriver.CustomCommandDefinitions);

            var parentFullName = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
            var localFile = parentFullName + "/Resources/ninja_saucebot-1.0-an+fx.xpi";
            var extensionByteArray = File.ReadAllBytes(localFile);
            var encodedExtension = Convert.ToBase64String(extensionByteArray);

            var installAddon = new Dictionary<string, object> { { "addon", encodedExtension } };
            var id = (string)customCommandDriver.ExecuteCustomDriverCommand(FirefoxDriver.InstallAddOnCommand, installAddon);
            
            _driver.Navigate().GoToUrl("https://www.saucedemo.com");

            Assert.IsTrue(_driver.FindElements(By.CssSelector(".bot_column2")).Count != 0);
            
            var removeAddon = new Dictionary<string, object> { { "id", id } };
            customCommandDriver.ExecuteCustomDriverCommand(FirefoxDriver.UninstallAddOnCommand, removeAddon);

            _driver.Navigate().Refresh();

            Assert.IsTrue(_driver.FindElements(By.CssSelector(".bot_column2")).Count == 0);
        }
    }
}
