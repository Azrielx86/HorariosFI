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

    public static async Task<int> GetSchedules(SchedulesDb db, int classCode)
    {
        var classesCount = 0;
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
            var header = table.SelectNodes("tr")[1]
                .SelectNodes("th")
                .Select(n => n.InnerText)
                .ToList();

            foreach (var tbody in table.SelectNodes("tbody"))
            {
                var rows = tbody.SelectNodes("tr").Count;
                var infoColumn = 0;
                var firstRow = tbody.SelectNodes("tr").First().ChildNodes;

                var code = header.Contains("Clave") ? int.Parse(firstRow[infoColumn++].InnerHtml) : -1;
                var group = header.Contains("Gpo") ? int.Parse(firstRow[infoColumn++].InnerHtml) : -1;
                var teacherName = header.Contains("Profesor") ? NameRegex().Replace(firstRow[infoColumn++].InnerHtml, string.Empty).Trim().ToTitle() : "NA";
                var type = header.Contains("Tipo") ? firstRow[infoColumn++].InnerHtml : "NA";
                var schedule = header.Contains("Horario") ? firstRow[infoColumn++].InnerHtml : "NA";
                var days = header.Contains("Días") ? firstRow[infoColumn++].InnerHtml : "NA";
                var classroom = header.Contains("Salón") ? firstRow[infoColumn++].InnerHtml : "NA";
                var quota = header.Contains("Cupo") ? int.Parse(firstRow[infoColumn++].InnerHtml) : -1;
                var vacancies = header.Contains("Vacantes") ? int.Parse(firstRow[infoColumn].InnerHtml) : -1;

                var fiClass = await db.FiClasses.FirstAsync(c => c.Code == code);

                var teacher = await db.FiTeachers.FirstOrDefaultAsync(t => t.Name.Equals(teacherName, StringComparison.CurrentCultureIgnoreCase));
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

                classesCount++;

                await db.SaveChangesAsync();
            }
        }

        return classesCount;
    }


    [GeneratedRegex(@"\d\d:\d\d")]
    private static partial Regex TimeRegex();

    [GeneratedRegex(@"<.*>|\(\w+\)|\(\w+\s\w+\)|\w+\.|\s\(\*.+\)?")]
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