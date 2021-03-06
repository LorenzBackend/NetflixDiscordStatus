using NetflixDiscordStatus.Misc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace NetflixDiscordStatus.Api
{
    public class Core
    {
        //NetflixDiscordStatus by LorenzBackend
        //https://github.com/LorenzBackend/NetflixDiscordStatus

        public static IWebDriver driver;
        private static WebDriverWait wait;

        private static string firefoxProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Mozilla\\Firefox\\Profiles\\5pqhihcu.default-release";
        public static void CheckNetflixPorifle()
        {
            try
            {
                string driverPath = Environment.CurrentDirectory + "\\lib\\";
                string profilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data";

                ChromeDriverService chromeSrv = ChromeDriverService.CreateDefaultService(driverPath);
                chromeSrv.HideCommandPromptWindow = true;

                ChromeOptions options = new ChromeOptions();
                //options.AddArgument("headless");
                options.AddArgument(@"user-data-dir=" + profilePath);

                driver = new ChromeDriver(chromeSrv, options);
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                driver.Navigate().GoToUrl("https://www.netflix.com/");

                Thread.Sleep(500);
                IWebElement btn = FindElementWhenExists(By.CssSelector(".profile-gate-label"));
                IWebElement dropDown = FindElementWhenExists(By.CssSelector(".account-dropdown-button"));

                if (btn != null || dropDown != null)
                {
                    NetflixState.Init();
                    MyEventHandler.SendInitResult("Shareing Netflix State", true);
                }
                else
                {
                    MyEventHandler.SendInitResult("looks like you are not logged into Netflix with Google Chrome", false);
                }
            }
            catch (Exception ex)
            {
                MyEventHandler.SendInitResult(ex.Message, false);
            }
        }

        public static string GetUrl()
        {        
            if (driver != null)
            {
                return driver.Url;
            }
            else
            {
                return null;
            }
 
        }
        public static IWebElement FindElementWhenExists(By by)
        {
            try
            {
                var elements = driver.FindElements(by);
                return (elements.Count >= 1) ? elements.First() : null;
            }
            catch
            {
                return null;
            }
            
        }

        public static string GetTextFromElementByCssWait(string css)
        {
            return WaitForElement(By.CssSelector(css)).Text;
        }

        public static IWebElement WaitForElement(By by)
        {
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(by));
        }

    }
}
