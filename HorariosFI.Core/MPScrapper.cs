using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using HorariosFI.Core.Extensions;
using HorariosFI.Core.Exeptions;

namespace HorariosFI.Core;

public class MPScrapper
{
    private const string MP_URL = "https://www.misprofesores.com/Buscar?buscar=Profesores&q={0}";
    private const string MP_LINK_XPATH = "//*[@id=\"___gcse_0\"]/div/div/div/div[5]/div[2]/div/div/div[1]/div[1]/div[1]/div[1]/div/a";

    private const string GENERAL_GRADE_XPATH = "/html/body/div[1]/div[2]/div/div/div[2]/div[1]/div/div[1]/div/div/div";
    //"//*[@id=\"mainContent\"]/div/div[2]/div[1]/div/div[1]/div/div/div";

    private const string RECOMMEND_XPATH = "/html/body/div[1]/div[2]/div/div/div[2]/div[1]/div/div[2]/div[1]/div";

    //"//*[@id=\"mainContent\"]/div/div[2]/div[1]/div/div[2]/div[1]/div";
    private const string DIFFICULTY_XPATH = "/html/body/div[1]/div[2]/div/div/div[2]/div[1]/div/div[2]/div[2]/div";

    //"//*[@id=\"mainContent\"]/div/div[2]/div[1]/div/div[2]/div[2]/div";
    private const int TIMEOUT = 3;

    public async Task Run(IEnumerable<ClassModel> classes)
    {
        await Task.Run(() =>
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("log-level=3");
            chromeOptions.AddArgument("headless");
            var chromeService = ChromeDriverService.CreateDefaultService();
            chromeService.HideCommandPromptWindow = true;
            using var driver = new ChromeDriver(chromeService, chromeOptions);

            var found = new Dictionary<string, ClassModel>();
            foreach (var classvar in classes)
            {
                if (found.ContainsKey(classvar.Profesor!))
                {
                    Console.WriteLine($"{classvar.Profesor} ya fue buscado.");
                    var p = found[classvar.Profesor!];
                    classvar.Grade = p.Grade;
                    classvar.Difficult = p.Difficult;
                    classvar.Recommend = p.Recommend;
                    classvar.MisProfesoresUrl = p.MisProfesoresUrl;
                    continue;
                }

                try
                {
                    var sleep = Random.Shared.Next(500, 4000);
                    Thread.Sleep(sleep);
                    var mpUrl = string.Format(MP_URL, classvar.Profesor);
                    driver.Navigate().GoToUrl(mpUrl);

                    var page = driver.FindElement(By.XPath(MP_LINK_XPATH), TIMEOUT) ?? throw new RobotDetectedException();
                    sleep = Random.Shared.Next(500, 4000);
                    Thread.Sleep(sleep);
                    page.Click();

                    var handles = driver.WindowHandles;
                    driver.SwitchTo().Window(handles[1]);

                    var generalGrade = driver.FindElement(By.XPath(GENERAL_GRADE_XPATH), TIMEOUT) ?? throw new Exception("No value");
                    var difficulty = driver.FindElement(By.XPath(DIFFICULTY_XPATH), TIMEOUT) ?? throw new Exception("No value");
                    var recommend = driver.FindElement(By.XPath(RECOMMEND_XPATH), TIMEOUT) ?? throw new Exception("No value");

                    classvar.Grade = double.Parse(generalGrade.Text);
                    classvar.Difficult = double.Parse(difficulty.Text);
                    classvar.Recommend = double.Parse(recommend.Text.Replace("%", ""));
                    classvar.MisProfesoresUrl = driver.Url;

                    Console.WriteLine(classvar.Profesor);
                    Console.WriteLine($"Grade: {generalGrade}\nDifficulty: {difficulty}\nRecommend: {recommend}");

                    found.Add(classvar.Profesor!, classvar);
                }
                catch (RobotDetectedException rb) { throw rb; }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex);
                    Console.ResetColor();
                }
                finally
                {
                    var handles = driver.WindowHandles;
                    driver.SwitchTo().Window(handles[1]);
                    driver.Close();
                    driver.SwitchTo().Window(handles[0]);
                }
            }
        });
    }
}