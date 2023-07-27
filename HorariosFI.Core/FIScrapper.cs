using HorariosFI.Core.Extensions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HorariosFI.Core;

public partial class FIScrapper
{
    private const string FI_URL = "https://www.ssa.ingenieria.unam.mx/cj/tmp/programacion_horarios/{0}.html?";
    private static readonly string[] days = { "Lun", "Mar", "Mie", "Jue", "Vie" };
    private static readonly string[] HEADER = { "Clave", "Gpo", "Profesor", "Tipo", "Horario", "Dias", "Cupo" };
    private static int NO_VAC_COLUMNS = 7;
    private static int VAC_COLUMNS = 8;

    public static async Task<(List<ClassModel>, IEnumerable<string>)> GetClassList(int class_code)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(string.Format(FI_URL, class_code));
        var content = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(content);

        var tables = document.DocumentNode.SelectNodes("//tbody");

        var result = new List<ClassModel>();
        var errors = new List<string>();
        if (tables is null || tables.Count == 0)
        {
            errors.Add($"Clase {class_code} no encontrada.");
            return (result, errors);
        }

        foreach (var (item, index) in tables.WithIndex())
        {
            try
            {
                if (item.ChildNodes.Count > 1) // Cuando hay más de un día o horario
                {
                    var data = item.SelectNodes("tr")
                                      .SelectMany(t => t.SelectNodes("td"))
                                      .Select(t => t.InnerText)
                                      .ToList();

                    if (data.Count >= 10)
                    {
                        data.Remove("L");
                    }
                    var times = data.Where(s => TimeRegex().IsMatch(s)).ToArray();
                    var days = data.Where(s => DayRegex().IsMatch(s)).ToArray();
                    data.RemoveAll(s => times.Contains(s) || days.Contains(s));
                    var cupo = data[^1];
                    data.RemoveAt(data.Count - 1);

                    data.Add(string.Join(" and ", times));
                    data.Add(string.Join(" and ", days));
                    data.Add(cupo);

                    if (data.Count != HEADER.Length) throw new Exception("Different sizes");
                    var paired = HEADER.Zip(data, (s, i) => new { s, i })
                            .ToDictionary(item => item.s, item => item.i);
                    result.Add(SerializeClass(paired));
                }
                else // Tabla normal
                {
                    var nodes = item.SelectNodes("tr").FirstOrDefault() ?? throw new Exception("Node not found");
                    var data = nodes.SelectNodes("td").Select(n => n.InnerHtml).ToArray();
                    if (data.Length != HEADER.Length) throw new Exception("Different sizes");
                    var paired = HEADER.Zip(data, (s, i) => new { s, i })
                            .ToDictionary(item => item.s, item => item.i);
                    result.Add(SerializeClass(paired));
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                errors.Add($"Falla al obtener: {item.InnerHtml}");
            }
        }
#if DEBUG
        result.ForEach(Console.WriteLine);
#endif
        return (result, errors);
    }

    [GeneratedRegex("\\d\\d:\\d\\d")]
    private static partial Regex TimeRegex();

    [GeneratedRegex("<\\w+>\\(\\*[\\w\\s\\D]+\\)</\\w+>|\\(\\w+\\)|<\\w+>|\\w+\\.")]
    private static partial Regex NameRegex();

    [GeneratedRegex("(\\w{3},\\s\\w{3})|(^[A-Z]{1}\\w{2}$)")]
    private static partial Regex DayRegex();

    private static ClassModel SerializeClass(Dictionary<string, string> paired)
    {
        return new ClassModel()
        {
            Profesor = NameRegex().Replace(paired["Profesor"], "").Trim(),
            Clave = int.Parse(paired["Clave"]),
            Cupo = int.Parse(paired["Cupo"]),
            Dias = paired["Dias"],
            Gpo = int.Parse(paired["Gpo"]),
            Tipo = paired["Tipo"],
            Vacantes = int.Parse(paired["Cupo"]),
            Horario = paired["Horario"]
        };
    }
}