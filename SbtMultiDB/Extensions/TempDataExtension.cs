using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SbtMultiDB;

/// <summary>
/// This class extends TempData to allow type-safe access to values,
/// eliminating the use of typo-prone strings as keys.
/// </summary>
static public class TempDataExtension
{
    const string TitleKey = "Title";
    const string CurrentOrganizationKey = "CurrentOrganization";
    const string EnableSubmitButtonKey = "EnableSubmitButton";

    public static void Title(this ITempDataDictionary tempData, string title)
    {
        tempData[TitleKey] = title;
    }

    public static string Title(this ITempDataDictionary tempData)
    {
        var returnValue = tempData[TitleKey] as string;

        if (returnValue == null)
        {
            return "";
        }
        return returnValue as string;
    }

    public static void CurrentOrganization(this ITempDataDictionary tempData, string currentOrganization)
    {
        tempData[CurrentOrganizationKey] = currentOrganization;
    }

    public static string CurrentOrganization(this ITempDataDictionary tempData)
    {
        var returnValue = tempData[CurrentOrganizationKey] as string;

        if (returnValue == null)
        {
            return "";
        }
        return returnValue as string;
    }
}
