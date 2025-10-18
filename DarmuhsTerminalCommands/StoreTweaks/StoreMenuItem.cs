using OpenLib.InteractiveMenus;
using System.Collections.Generic;
using System.Linq;
using TerminalStuff.CommandHandling;
using TerminalStuff.SpecialStuff;
using TerminalStuff.Util;
using static OpenLib.Events.Events;

namespace TerminalStuff.StoreTweaks;

public class StoreMenuItem(string name) : MenuItem(StorePlus.StorePlusMenu)
{
    private string _name = name;
    public override string Name
    {
        get => _name;
        set => _name = value;
    }

    private bool _showEmpty = false;
    public override bool ShowIfEmptyNest
    {
        get
        {
            if (StoreItem == null)
                return _showEmpty;
            else
                return StoreItem.ShouldShowInStore();
        }
        set => _showEmpty = value;
    }

    private CustomEvent _selection = new();
    public override CustomEvent SelectionEvent
    {
        get => _selection;
        set => _selection = value;
    }

    private List<MenuItem> _nested = [];
    public override List<MenuItem> NestedMenus
    {
        get => _nested;
        set => _nested = value;
    }

    internal string keyword = "";

    public StoreInfo StoreItem = null!;

    public StoreMenuItem(string name, string kw, MenuItem Parent) : this(name)
    {
        Name = name;
        keyword = kw;
        SetParentMenu(Parent);
        SelectionEvent.AddListener(OnExternalSelect);
        LoadPageOnSelect = false;
        ShowIfEmptyNest = true;
    }

    public StoreMenuItem(string name, bool showEmpty) : this(name)
    {
        Name = name;
        ShowIfEmptyNest = showEmpty;
    }

    public static bool DoesMenuItemExist(string keyword)
    {
        if (StorePlus.StorePlusMenu.AllMenuItemsOfType.Count == 0)
            return false;

        if (StorePlus.StorePlusMenu.AllMenuItemsOfType.ConvertAll(x => x as StoreMenuItem).Any(x => x?.keyword == keyword))
            return true;

        return false;
    }

    public static bool DoesMenuItemExist(StoreInfo item)
    {
        if (StorePlus.StorePlusMenu.AllMenuItemsOfType.Count == 0)
            return false;

        if (StorePlus.StorePlusMenu.AllMenuItemsOfType.ConvertAll(x => x as StoreMenuItem).Any(x => x?.StoreItem == item))
            return true;

        return false;
    }

    public void OnExternalSelect()
    {
        if (keyword.Length < 2)
            return;

        StorePlus.StorePlusMenu.ExitAction = () =>
        {
            StartofHandling.HandleShortcutFinal(keyword);
            Loggers.LogDebug($"Selecting external keyword! {keyword}");
        };
        StorePlus.StorePlusMenu.ExitMenu(true);
    }
}
