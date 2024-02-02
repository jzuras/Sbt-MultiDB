using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests;

internal class SeleniumWrapper
{
    internal static void SendKeys(IWebDriver driver, By by, string valueToType)
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        try
        {
            IWebElement? element = wait.Until(driver => FindElementWithRetry(driver, by));

            // Check the type attribute of the input element.
            string inputType = element?.GetAttribute("type") ?? "";

            element?.Clear();
            element?.SendKeys(valueToType);
        }
        catch (WebDriverTimeoutException)
        {
            Assert.Fail($"Exception in SendKeys(): element located by {by.ToString()} not visible and enabled within {5} seconds.");
        }
    }

    internal static void ClickWithRetry(IWebDriver driver, By by)
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        IWebElement? element = null;
        bool? tmp1, tmp2;

        try
        {
            element = wait.Until(driver => FindElementWithRetry(driver, by));
            tmp1 = element?.Displayed;
            tmp2 = element?.Enabled;
            element?.Click();
        }
        catch (WebDriverTimeoutException)
        {
            Assert.Fail($"Exception in Click(): element located by {by.ToString()} not visible and enabled within {5} seconds.");
        }
        catch (ElementNotInteractableException)
        {
            // Sometimes a driver scrolls correctly but needs to wait a beat before clicking.
            System.Threading.Thread.Sleep(1000);
            try
            {
                element?.Click();
            }
            catch
            {
                // And sometimes we have to try one more time.
                System.Threading.Thread.Sleep(1000);
                element?.Click();
            }
        }
    }

    internal static IWebElement? FindElementWithRetry(IWebDriver driver, By by)
    {
        try
        {
            IWebElement? element = driver.FindElement(by);
            return (element?.Displayed ?? false) && (element?.Enabled ?? false) ? element : null;
        }
        catch
        {
            // Try one more time.
            IWebElement? element = driver.FindElement(by);
            return (element?.Displayed ?? false) && (element?.Enabled ?? false) ? element : null;
        }
    }
}
