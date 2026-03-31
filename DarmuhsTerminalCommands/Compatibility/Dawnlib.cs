using Dawn;
using Dawn.Internal;
using UnityEngine;

namespace TerminalStuff.Compatibility;

internal class Dawnlib
{
    internal static bool UseLLLInstead()
    {
        if (!Plugin.instance.LethalLevelLoader)
            return false;

        if (!Plugin.instance.DawnLibPresent)
            return true;

        return DawnConfig.AllowLLLToOverrideVanillaStatus.Value;
    }

    internal static int GetPrice(SelectableLevel level)
    {
        if (!Plugin.instance.DawnLibPresent)
            return 0;

        DawnMoonInfo moon = level.GetDawnInfo();

        return Mathf.Max(0, moon.DawnPurchaseInfo.Cost.Provide());
    }

    internal static bool IsLocked(SelectableLevel level)
    {
        if (!Plugin.instance.DawnLibPresent)
            return false;

        DawnMoonInfo moon = level.GetDawnInfo();
        TerminalPurchaseResult result = moon.DawnPurchaseInfo.PurchasePredicate.CanPurchase();

        if (result is TerminalPurchaseResult.HiddenPurchaseResult hiddenResult)
        {
            return hiddenResult.IsFailure;
        }
        else
        {
            return result is TerminalPurchaseResult.FailedPurchaseResult;
        }
    }

    internal static bool IsHidden(SelectableLevel level)
    {
        if (!Plugin.instance.DawnLibPresent)
            return false;

        DawnMoonInfo moon = level.GetDawnInfo();
        return moon.DawnPurchaseInfo.PurchasePredicate.CanPurchase() is TerminalPurchaseResult.HiddenPurchaseResult;
    }

    internal static bool IsDisabled(SelectableLevel level)
    {
        if (!Plugin.instance.DawnLibPresent)
            return false;

        DawnMoonInfo moon = level.GetDawnInfo();

        return moon.DawnPurchaseInfo == null;
    }

    internal static void ChangeHiddenStatus(SelectableLevel level, bool shouldHide)
    {
        if (!Plugin.instance.DawnLibPresent)
            return;

        DawnMoonInfo moon = level.GetDawnInfo();

        if (!shouldHide)
            moon.DawnPurchaseInfo.PurchasePredicate = ITerminalPurchasePredicate.AlwaysSuccess();
        else
            moon.DawnPurchaseInfo.PurchasePredicate = Hidden;
    }

    // Unused, would probably need a method to relock/hide on lobby reset
    internal static void UnlockUnhide(SelectableLevel level)
    {
        if (!Plugin.instance.DawnLibPresent)
            return;

        DawnMoonInfo moon = level.GetDawnInfo();

        moon.DawnPurchaseInfo.PurchasePredicate = ITerminalPurchasePredicate.AlwaysSuccess();
    }

    private static object _hidden = null!;
    private static ITerminalPurchasePredicate Hidden
    {
        get
        {
            if (_hidden as ITerminalPurchasePredicate is null)
                _hidden = new ConstantTerminalPredicate(new TerminalPurchaseResult.HiddenPurchaseResult().SetFailure(false));

            return (ITerminalPurchasePredicate)_hidden;
        }
    }

}
