using System.Text;

namespace ReadyErp.Api.Services;

public static class AmountInWords
{
    private static readonly string[] Ones =
    {
        "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
        "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
        "Seventeen", "Eighteen", "Nineteen"
    };

    private static readonly string[] Tens =
    {
        "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
    };

    public static string Convert(decimal amount)
    {
        var whole = (long)Math.Floor(amount);
        var result = ConvertWhole(whole);
        return $"BDT {result} Only";
    }

    private static string ConvertWhole(long number)
    {
        if (number == 0) return "Zero";

        var sb = new StringBuilder();

        sb.Append(AppendSegment(number / 10000000, "Crore"));
        number %= 10000000;
        sb.Append(AppendSegment(number / 100000, "Lakh"));
        number %= 100000;
        sb.Append(AppendSegment(number / 1000, "Thousand"));
        number %= 1000;
        sb.Append(AppendSegment(number / 100, "Hundred"));
        number %= 100;

        if (number > 0)
        {
            if (sb.Length > 0) sb.Append("and ");
            sb.Append(TwoDigits(number));
        }

        return sb.ToString().Trim();
    }

    private static string AppendSegment(long value, string label)
    {
        if (value == 0) return string.Empty;
        return $"{TwoDigits(value)} {label} ";
    }

    private static string TwoDigits(long number)
    {
        if (number < 20) return Ones[number];
        var tens = Tens[number / 10];
        var ones = Ones[number % 10];
        return string.IsNullOrEmpty(ones) ? tens : $"{tens} {ones}";
    }
}
