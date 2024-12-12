using HorariosFI.Core.Extensions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using HorariosFI.Core.Exeptions;

namespace HorariosFI.Core;

public static partial class FiScrapper
{
#if DEBUG
    private const string FiUrl = "http://localhost:8000/{0}.html?";
#else
    private const string FiUrl = "https://www.ssa.ingenieria.unam.mx/cj/tmp/programacion_horarios/{0}.html?";
#endif
    private const string InfoUrl =
        "https://www.dgae-siae.unam.mx/educacion/asignaturas.php?ref=asgxfrm&asg={0}&plt=0011";

    public static async Task<string?> GetClassName(int classCode)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(string.Format(InfoUrl, classCode));
        var content = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(content);

        var nameNode = document.DocumentNode.Descendants(0).FirstOrDefault(n => n.HasClass("post-titulo"));
        if (nameNode is null)
            return null;

        var name = CleanNameRegex().Replace(nameNode.InnerHtml, "");

        return name.ToTitle();
    }

    public static async Task<List<ClassModel>> GetClassShcedules(int classCode)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(string.Format(FiUrl, classCode));
        var content = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(content);

        var tables = document.DocumentNode.SelectNodes("//table");
        if (tables is null || tables.Count == 0)
            throw new ClassNotFoundException();

        var result = new List<ClassModel>();

        foreach (var table in tables)
        {
            // Localización del header
            var header = table.SelectNodes("tr")[1].SelectNodes("th").Select(n => n.InnerText).ToList();

            // Selección de los demás elementos
            var profNodes = table.SelectNodes("tbody").ToList();
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var profInfo in profNodes)
            {
                var data = profInfo.SelectNodes("tr/td").Select(n => n.InnerText).ToList();
                if (data.Remove("L")) data[data.IndexOf("T")] = "T/L";
                var times = data.Where(s => TimeRegex().IsMatch(s)).ToArray();
                var days = data.Where(s => DayRegex().IsMatch(s)).ToArray();
                data.RemoveAll(s => times.Contains(s) || days.Contains(s));

                header.Remove("Días");
                header.Add("Días");
                data.Add(string.Join(" | ", days));

                header.Remove("Horario");
                header.Add("Horario");
                data.Add(string.Join(" | ", times));

                if (data.Count != header.Count) throw new Exception("Different sizes");
                var paired = header.Zip(data, (s, i) => new { s, i })
                    .ToDictionary(item => item.s, item => item.i);

                paired["Profesor"] = NameRegex().Replace(paired["Profesor"], string.Empty);

                var obj = ClassModel.CreateFromDictionary(paired);
                result.Add(obj);
            }
        }

        return result;
    }

    [GeneratedRegex(@"\d\d:\d\d")]
    private static partial Regex TimeRegex();

    [GeneratedRegex(@"<\w+>\(\*[\w\s\D]+\)</\w+>|\(\w+\)|\(\w+\s\w+\)|<\w+>|\w+\.|\s\(\*.+\)?")]
    private static partial Regex NameRegex();

    [GeneratedRegex(@"((\w{3},\s\w{3})|(^[A-Z]{1}\w{2}$))")]
    private static partial Regex DayRegex();

    [GeneratedRegex(@"^\w+:\s\[\d+\]\s\-\s")]
    private static partial Regex CleanNameRegex();
}