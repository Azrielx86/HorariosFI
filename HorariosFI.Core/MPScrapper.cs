using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace HorariosFI.Core;

public class MPScrapper
{
    public void Run(IEnumerable<ClassModel> classes)
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("log-level=3");
        chromeOptions.AddArgument("headless");
        var chromeService = ChromeDriverService.CreateDefaultService();
        chromeService.HideCommandPromptWindow = true;
        using var driver = new ChromeDriver(chromeService, chromeOptions);

        foreach (var classvar in classes)
        {
            var mpUrl = string.Format(MPUrls.MP_URL, classvar.Name);
            driver.Navigate().GoToUrl(mpUrl);

            var page = driver.FindElement(By.XPath(MPUrls.MP_LINK_XPATH));
            page.Click();

            var handles = driver.WindowHandles;
            driver.SwitchTo().Window(handles[1]);

            var generalGrade = driver.FindElement(By.XPath(MPUrls.GENERAL_GRADE_XPATH)).Text;
            var difficulty = driver.FindElement(By.XPath(MPUrls.DIFFICULTY_XPATH)).Text;
            var recommend = driver.FindElement(By.XPath(MPUrls.RECOMMEND_XPATH)).Text;

            try
            {
                classvar.Grade = double.Parse(generalGrade);
                classvar.Difficult = double.Parse(difficulty);
                classvar.Recommend = double.Parse(recommend.Replace("%", ""));
                classvar.MisProfesoresUrl = driver.Url;
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ResetColor();
            }

            Console.WriteLine(classvar.Name);
            Console.WriteLine($"Grade: {generalGrade}\nDifficulty: {difficulty}\nRecommend: {recommend}");

            driver.Close();
            driver.SwitchTo().Window(handles[0]);
        }
    }
}