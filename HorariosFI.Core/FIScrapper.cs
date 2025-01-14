using HorariosFI.Core.Extensions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using HorariosFI.Core.Exeptions;
using HorariosFI.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HorariosFI.Core;

public static partial class FiScrapper
{
    /*
     * For development purposes and to avoid doing too many requests to the faculty page.
     * You can get the schedules manually with `curl` and save it with the format <class_code>.html
     * For example, in a example directory
     * .
     * |-- 119.html
     * |-- 1590.html
     *
     * And then you can start a local server with python using the following command:
     *
     *      python -m http.server 8000
     */
#if DEBUG
    private const string FiUrl = "http://localhost:8000/{0}.html?";
#else
    private const string FiUrl = "https://www.ssa.ingenieria.unam.mx/cj/tmp/programacion_horarios/{0}.html?";
#endif
    private const string InfoUrl =
        "https://www.dgae-siae.unam.mx/educacion/asignaturas.php?ref=asgxfrm&asg={0}&plt=0011";

    /// <summary>
    /// Returns the class name doing a search in the DGAE page.
    /// </summary>
    /// <param name="classCode">Code given in the career curriculum</param>
    /// <returns>String with the class name if exists, else null</returns>
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

        var name = ClassNameRegex().Replace(nameNode.InnerHtml, "");

        return name.ToTitle();
    }

    public static async Task GetSchedules(SchedulesDb db, int classCode)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(string.Format(FiUrl, classCode));
        var content = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(content);

        var tables = document.DocumentNode.SelectNodes("//table");
        if (tables is null || tables.Count == 0)
            throw new ClassNotFoundException(classCode);

        foreach (var table in tables)
        {
            foreach (var tbody in table.SelectNodes("tbody"))
            {
                var rows = tbody.SelectNodes("tr").Count;
                var firstRow = tbody.SelectNodes("tr").First().ChildNodes;

                var code = int.Parse(firstRow[0].InnerHtml);
                var group = int.Parse(firstRow[1].InnerHtml);
                var teacherName = NameRegex().Replace(firstRow[2].InnerHtml, string.Empty).Trim();
                var type = firstRow[3].InnerHtml;
                var schedule = firstRow[4].InnerHtml;
                var days = firstRow[5].InnerHtml;
                var classroom = firstRow[6].InnerHtml;
                var quota = int.Parse(firstRow[7].InnerHtml);

                var vacancies = firstRow.Count > 7 ? int.Parse(firstRow[8].InnerHtml) : 0;

                var fiClass = await db.FiClasses.FirstAsync(c => c.Code == code);

                var teacher = await db.FiTeachers.FirstOrDefaultAsync(t => t.Name == teacherName);
                if (teacher is null)
                {
                    teacher = new FiTeacher { Name = teacherName };
                    db.FiTeachers.Add(teacher);
                }

                var groupRegister = await db.FiGroups.FirstOrDefaultAsync(g => g.FiTeacher == teacher && g.Group == group);
                if (groupRegister is null)
                {
                    groupRegister = new FiGroup
                    {
                        Group = group,
                        Classroom = classroom,
                        Days = days,
                        Quota = quota,
                        Type = type,
                        Vacancies = vacancies,
                        Schedules = schedule,
                        FiTeacherId = teacher.Id,
                        FiClassId = fiClass.Code
                    };
                    db.FiGroups.Add(groupRegister);
                }

                // Case for T/L type or more classrooms
                if (rows > 1)
                {
                    var lastRow = tbody.SelectNodes("tr").Last().ChildNodes;

                    var typeNode = lastRow.FirstOrDefault(r => TypeRegex().IsMatch(r.InnerHtml));
                    if (typeNode is not null)
                    {
                        type = typeNode.InnerHtml;
                        lastRow.Remove(typeNode);
                    }

                    var classroomNode = lastRow.FirstOrDefault(r => ClassrooomRegex().IsMatch(r.InnerHtml));
                    if (classroomNode is not null)
                    {
                        classroom = classroomNode.InnerHtml;
                        lastRow.Remove(classroomNode);
                    }

                    var hoursNode = lastRow.FirstOrDefault(r => TimeRegex().IsMatch(r.InnerHtml));
                    if (hoursNode is not null)
                    {
                        schedule = hoursNode.InnerHtml;
                        lastRow.Remove(hoursNode);
                    }

                    var daysNode = lastRow.FirstOrDefault(r => DayRegex().IsMatch(r.InnerHtml));
                    if (daysNode is not null)
                    {
                        days = daysNode.InnerHtml;
                        lastRow.Remove(daysNode);
                    }

                    groupRegister = new FiGroup
                    {
                        Group = group,
                        Classroom = classroom,
                        Days = days,
                        Quota = quota,
                        Type = type,
                        Vacancies = vacancies,
                        Schedules = schedule,
                        FiTeacherId = teacher.Id,
                        FiClassId = fiClass.Code
                    };
                    db.FiGroups.Add(groupRegister);
                }

                await db.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Gets all the schedules from the faculty page, using the url which returns a table with all the data,
    /// whithout interacting with the page.
    /// </summary>
    /// <param name="classCode">Code given in the career curriculum</param>
    /// <returns>List with the data parsed in <see cref="ClassModel"/> objects</returns>
    /// <exception cref="ClassNotFoundException">Thrown by a incorrect class code</exception>
    /// <exception cref="FiScrapperException">Thrown when incorrect data is scraped</exception>
    [Obsolete("Will be replaced in the future", false)]
    public static async Task<List<ClassModel>> GetClassShcedules(int classCode)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(string.Format(FiUrl, classCode));
        var content = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(content);

        var tables = document.DocumentNode.SelectNodes("//table");
        if (tables is null || tables.Count == 0)
            throw new ClassNotFoundException(classCode);

        var result = new List<ClassModel>();

        foreach (var table in tables)
        {
            // Localización del header
            var header = table.SelectNodes("tr")[1]
                .SelectNodes("th")
                .Select(n => n.InnerText)
                .ToList();

            // Selección de los demás elementos
            var profNodes = table.SelectNodes("tbody").ToList();
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var profInfo in profNodes)
            {
                var data = profInfo.SelectNodes("tr/td").Select(n => n.InnerText).ToList();
                if (data.Remove("L")) data[data.IndexOf("T")] = "T/L";
                var times = data.Where(s => TimeRegex().IsMatch(s));
                var days = data.Where(s => DayRegex().IsMatch(s));
                var classrooms = data.Where(s => ClassrooomRegex().IsMatch(s));
                data.RemoveAll(s => times.Contains(s) || days.Contains(s) || classrooms.Contains(s));

                header.Remove("Días");
                header.Add("Días");
                data.Add(string.Join(" | ", days));

                header.Remove("Salón");
                header.Add("Salón");
                data.Add(string.Join(" | ", classrooms));

                header.Remove("Horario");
                header.Add("Horario");
                data.Add(string.Join(" | ", times));

                if (data.Count != header.Count) throw new FiScrapperException("Different sizes");
                var paired = header.Zip(data, (s, i) => new { s, i })
                    .ToDictionary(item => item.s, item => item.i);

                paired["Profesor"] = NameRegex().Replace(paired["Profesor"], string.Empty).Trim();

                result.Add(paired);
            }
        }

        return result;
    }

    // private static void GetTClass(){}
    // private static void GetTLClass(){}
    // private static void GetTLPlusClass(){}


    [GeneratedRegex(@"\d\d:\d\d")]
    private static partial Regex TimeRegex();

    [GeneratedRegex(@"<\w+>\(\*[\w\s\D]+\)</\w+>|\(\w+\)|\(\w+\s\w+\)|<\w+>|\w+\.|\s\(\*.+\)?")]
    private static partial Regex NameRegex();

    [GeneratedRegex(@"((\w{3},\s\w{3})|(^[A-Z]{1}\w{2}$))")]
    private static partial Regex DayRegex();

    [GeneratedRegex(@"^\w+:\s\[\d+\]\s\-\s")]
    private static partial Regex ClassNameRegex();

    [GeneratedRegex(@"[A-Z]\d{3}")]
    private static partial Regex ClassrooomRegex();

    [GeneratedRegex(@"^(T\+?|L)$")]
    private static partial Regex TypeRegex();
}