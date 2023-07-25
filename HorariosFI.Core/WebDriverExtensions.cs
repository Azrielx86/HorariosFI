using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace HorariosFI.Core;

public static class WebDriverExtensions
{
    public static IWebElement FindElement(this IWebDriver driver, By by, int timeout = 20)
    {
        if (timeout > 0)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
            return wait.Until(drv => drv.FindElement(by));
        }
        return driver.FindElement(by);
    }
}