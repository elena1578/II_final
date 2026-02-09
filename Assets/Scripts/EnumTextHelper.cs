using UnityEngine;
using System.Text.RegularExpressions;

public static class EnumTextHelper
{
    /// <summary>
    /// call this instead of ToString() on enums to ensure space is placed btwn combined words.
    /// e.g., BreadSlice -> Bread Slice
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToDisplayName(this System.Enum value)
    {
        // e.g., BreadSlide -> Bread Slice
        return Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");
    }
}
