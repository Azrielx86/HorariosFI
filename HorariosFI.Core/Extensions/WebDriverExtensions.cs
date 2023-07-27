using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace HorariosFI.Core.Extensions;

public static class WebDriverExtensions
{
    public static IWebElement FindElement(this ChromeDriver driver, By by, int timeout = 20)
    {
        try
        {
            if (timeout > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }
        catch (Exception)
        {
            return null!;
        }
    }
}