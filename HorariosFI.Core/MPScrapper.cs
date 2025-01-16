using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using HorariosFI.Core.Extensions;
using HorariosFI.Core.Exeptions;
using HorariosFI.Core.Models;

namespace HorariosFI.Core;

public static class MpScrapper
{
    private const string MpUrl = "http://www.misprofesores.com/Buscar?buscar=Profesores&q={0}";

    private const string MpLinkXpath =
        "//*[@id=\"___gcse_0\"]/div/div/div/div[5]/div[2]/div/div/div[1]/div[1]/div[1]/div[1]/div/a";

    private const string GeneralGradeXpath = "/html/body/div[1]/div[2]/div/div/div[2]/div[1]/div/div[1]/div/div/div";
    //"//*[@id=\"mainContent\"]/div/div[2]/div[1]/div/div[1]/div/div/div";

    private const string RecommendXpath = "/html/body/div[1]/div[2]/div/div/div[2]/div[1]/div/div[2]/div[1]/div";

    //"//*[@id=\"mainContent\"]/div/div[2]/div[1]/div/div[2]/div[1]/div";
    private const string DifficultyXpath = "/html/body/div[1]/div[2]/div/div/div[2]/div[1]/div/div[2]/div[2]/div";

    //"//*[@id=\"mainContent\"]/div/div[2]/div[1]/div/div[2]/div[2]/div";
    private const int Timeout = 3;


    public static async Task Run(IList<FiClassModel> classes, IProgress<int> progress, bool showWindow = false)
    {
        await Task.Run(() =>
        {
            var chromeOptions = new ChromeOptions
            {
                AcceptInsecureCertificates = true
            };
            chromeOptions.AddArgument("--test-type");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            if (!showWindow)
            {
                chromeOptions.AddArgument("--log-level=3");
                chromeOptions.AddArgument("--headless");
            }

            var chromeService = ChromeDriverService.CreateDefaultService();
            chromeService.HideCommandPromptWindow = true;
            using var driver = new ChromeDriver(chromeService, chromeOptions);

            var found = new Dictionary<string, FiClassModel>();
            foreach (var (classvar, index) in classes.WithIndex())
            {
                var p = index * 100 / classes.Count;
                progress.Report(p);

                if (found.TryGetValue(classvar.Teacher!, out var value))
                {
                    classvar.Grade = value.Grade;
                    classvar.Difficult = value.Difficult;
                    classvar.Recommend = value.Recommend;
                    classvar.MisProfesoresUrl = value.MisProfesoresUrl;
                    continue;
                }

                try
                {
                    var sleep = Random.Shared.Next(500, 4000);
                    Thread.Sleep(sleep);
                    var mpUrl = string.Format(MpUrl, classvar.Teacher);
                    driver.Navigate().GoToUrl(mpUrl);

                    var page = driver.FindElement(By.XPath(MpLinkXpath), Timeout) ?? throw new RobotDetectedException();
                    sleep = Random.Shared.Next(500, 4000);
                    Thread.Sleep(sleep);
                    page.Click();

                    var handles = driver.WindowHandles;
                    driver.SwitchTo().Window(handles[1]);

                    var generalGrade = driver.FindElement(By.XPath(GeneralGradeXpath), Timeout) ??
                                       throw new Exception("No value");
                    var difficulty = driver.FindElement(By.XPath(DifficultyXpath), Timeout) ??
                                     throw new Exception("No value");
                    var recommend = driver.FindElement(By.XPath(RecommendXpath), Timeout) ??
                                    throw new Exception("No value");

                    classvar.Grade = double.Parse(generalGrade.Text);
                    classvar.Difficult = double.Parse(difficulty.Text);
                    classvar.Recommend = double.Parse(recommend.Text.Replace("%", ""));
                    classvar.MisProfesoresUrl = driver.Url;

                    found.Add(classvar.Teacher!, classvar);
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