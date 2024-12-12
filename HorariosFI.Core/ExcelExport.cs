using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using HorariosFI.Core.Extensions;

namespace HorariosFI.Core;

public enum PropIndex
{
    Ignore,
    Clave,
    Group,
    Name,
    Grade,
    Difficult,
    Recommend,
    Tipo,
    Horario,
    Dias,
    Cupo,
    MpUrl,
}

public partial class ExcelExport
{
    private const string Filename = "Horarios_FI.xlsx";

    [SuppressMessage("ReSharper", "SpecifyACultureInStringConversionExplicitly")]
    public static void Export(int clave, string name, List<ClassModel>? classes)
    {
        if (classes is null) return;

        var document = File.Exists(Filename) ? new XLWorkbook(Filename) : new XLWorkbook();

        // var wsName = $"{clave}-{ReplaceSpaces().Replace(name, " ")}"[..(name.Length > 25 ? 25 : name.Length)];
        var wsName = $"{clave}-{ReplaceSpaces().Replace(name, " ")}"[..(name.Length > 31 ? 31 : name.Length)].Replace(":", string.Empty);

        if (!document.TryGetWorksheet(wsName, out var worksheet))
            worksheet = document.AddWorksheet(wsName);

        worksheet.Cell(1, (int)PropIndex.Clave).SetValue("Clave");
        worksheet.Cell(1, (int)PropIndex.Group).SetValue("Grupo");
        worksheet.Cell(1, (int)PropIndex.Name).SetValue("Nombre");
        worksheet.Cell(1, (int)PropIndex.Grade).SetValue("Grado");
        worksheet.Cell(1, (int)PropIndex.Difficult).SetValue("Dificultad");
        worksheet.Cell(1, (int)PropIndex.Recommend).SetValue("Recomendado");
        worksheet.Cell(1, (int)PropIndex.Tipo).SetValue("Tipo");
        worksheet.Cell(1, (int)PropIndex.Horario).SetValue("Horario");
        worksheet.Cell(1, (int)PropIndex.Dias).SetValue("Días");
        worksheet.Cell(1, (int)PropIndex.Cupo).SetValue("Cupo");
        worksheet.Cell(1, (int)PropIndex.MpUrl).SetValue("Link");

        worksheet.Column((int)PropIndex.Name).Width = 30;
        worksheet.Column((int)PropIndex.MpUrl).Width = 60;

        foreach (var (item, index) in classes.WithIndex())
        {
            worksheet.Cell(index + 2, (int)PropIndex.Clave).SetValue(item.Clave.ToString());
            worksheet.Cell(index + 2, (int)PropIndex.Group).SetValue(item.Gpo.ToString());
            worksheet.Cell(index + 2, (int)PropIndex.Name).SetValue(item.Profesor);
            worksheet.Cell(index + 2, (int)PropIndex.Tipo).SetValue(item.Tipo);
            worksheet.Cell(index + 2, (int)PropIndex.Horario).SetValue(item.Horario);
            worksheet.Cell(index + 2, (int)PropIndex.Dias).SetValue(item.Dias);
            worksheet.Cell(index + 2, (int)PropIndex.Cupo).SetValue(item.Cupo.ToString());

            // Special Cells
            worksheet.Cell(index + 2, (int)PropIndex.MpUrl).SetValue(item.MisProfesoresUrl ?? "NA");

            var gradeCell = worksheet.Cell(index + 2, (int)PropIndex.Grade);
            gradeCell.Style.Fill.BackgroundColor = item.Grade == null ? XLColor.LightGray : item.Grade <= 5 ? XLColor.LightCoral : item.Grade <= 7 ? XLColor.Khaki : XLColor.LightGreen;
            gradeCell.Value = TryParseValue(item.Grade, "NA");

            var difCell = worksheet.Cell(index + 2, (int)PropIndex.Difficult);
            difCell.Style.Fill.BackgroundColor = item.Difficult == null ? XLColor.LightGray : item.Difficult >= 4 ? XLColor.LightCoral : item.Difficult >= 2.5 ? XLColor.Khaki : XLColor.LightGreen;
            difCell.Value = TryParseValue(item.Difficult, "NA");

            var recCell = worksheet.Cell(index + 2, (int)PropIndex.Recommend);
            recCell.Style.Fill.BackgroundColor = item.Recommend == null ? XLColor.LightGray : item.Recommend <= 50 ? XLColor.LightCoral : item.Recommend <= 70 ? XLColor.Khaki : XLColor.LightGreen;
            recCell.Value = TryParseValue(item.Recommend, "NA");
        }

        document.SaveAs(Filename);
    }

    private static string TryParseValue<T>(T? value, string @default)
    {
        string result;
        try
        {
            if (value is null) throw new Exception();
            result = value.ToString() ?? @default;
        }
        catch (Exception)
        {
            result = @default;
        }

        return result;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex ReplaceSpaces();
}