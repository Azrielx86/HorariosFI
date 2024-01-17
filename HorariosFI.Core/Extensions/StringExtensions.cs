using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static System.Char;

namespace HorariosFI.Core.Extensions;

public static class StringExtensions
{
    public static string ToTitle(this string str)
    {
        var lowerText = str.ToLower().ToCharArray();
        foreach (Match match in Regex.Matches(str, @"\b[A-Za-z]"))
            lowerText[match.Index] = ToUpper(lowerText[match.Index]);
        return new string(lowerText);
    }

    public static string RemoveDiacritics(this string str)
    {
        var text = str.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray();
        return new string(text);
    }
}