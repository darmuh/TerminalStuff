
namespace TerminalStuff.SpecialStuff.Store;

public class StoreSettings
{
    public static int savings = 0;
    public static StoreMenuItem settings = new("Settings", true);
    public static StoreMenuItem sortAbc = new("Sort Alphabetically", true);
    public static StoreMenuItem sort123 = new("Sort By Price", true);
    public static StoreMenuItem sortNone = new("Remove Sorting", true);
    public static StoreMenuItem keepCreds = new($"Increase Savings: <color=#1cde2b>${savings}</color> (5%)", true);
    public static StoreMenuItem clearSavings = new($"Clear Savings", true);
    public static SortingStyle SortStyle = new();


    public enum SortingStyle
    {
        None = 0,
        AlphabeticalDown = 1,
        AlphabeticalUp = 2,
        PriceDown = 3,
        PriceUp = 4
    }

    internal static void Init()
    {
        settings.SetParentMenu(StorePlus.TheMainMenu);
        sortAbc.SetParentMenu(settings);
        sortAbc.SelectionEvent.AddListener(SetSortAbc);
        sort123.SetParentMenu(settings);
        sort123.SelectionEvent.AddListener(SetSort123);
        sortNone.SetParentMenu(settings);
        sortNone.SelectionEvent.AddListener(SortNone);
        keepCreds.SetParentMenu(settings);
        keepCreds.SelectionEvent.AddListener(ToggleSavings);
        clearSavings.SetParentMenu(settings);
        clearSavings.SelectionEvent.AddListener(ClearSavings);
        SortStyle = SortingStyle.None;
        settings.AdditionalBottomText = $"Current Sort: [{SortStyle}]\r\n\r\n";
    }

    private static void SortNone()
    {
        SortStyle = SortingStyle.None;
        settings.AdditionalBottomText = $"Current Sort: [{SortStyle}]\r\n\r\n";
    }

    private static void SetSortAbc()
    {
        if (SortStyle == SortingStyle.AlphabeticalDown)
            SortStyle = SortingStyle.AlphabeticalUp;
        else if (SortStyle == SortingStyle.AlphabeticalUp)
            SortStyle = SortingStyle.AlphabeticalDown;
        else
            SortStyle = SortingStyle.AlphabeticalDown;

        settings.AdditionalBottomText = $"Current Sort: [{SortStyle}]\r\n\r\n";
    }

    private static void SetSort123()
    {
        if (SortStyle == SortingStyle.PriceDown)
            SortStyle = SortingStyle.PriceUp;
        else if (SortStyle == SortingStyle.PriceUp)
            SortStyle = SortingStyle.PriceDown;
        else
            SortStyle = SortingStyle.PriceDown;

        settings.AdditionalBottomText = $"Current Sort: [{SortStyle}]\r\n\r\n";
    }

    private static void ToggleSavings()
    {
        int fullCreds = Plugin.instance.Terminal.groupCredits - StorePlus.SubTotal;

        float mathresult = (float)Plugin.instance.Terminal.groupCredits * 0.05f;

        if (savings + (int)mathresult <= fullCreds)
            savings += (int)mathresult;
        else
            return;

        keepCreds.Name = $"Increase Savings: <color=#1cde2b>${savings}</color> (5%)";
    }

    private static void ClearSavings()
    {
        savings = 0;
        keepCreds.Name = $"Increase Savings: <color=#1cde2b>${savings}</color> (5%)";
    }
}
