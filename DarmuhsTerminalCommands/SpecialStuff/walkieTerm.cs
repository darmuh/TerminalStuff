using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace TerminalStuff
{
    public class WalkieTerm : MonoBehaviour
    {
        //static PlayerControllerB getmyself = GameNetworkManager.Instance.localPlayerController;

        public static string UseWalkieKey = ConfigSettings.WalkieTermKey.Value;
        public static string UseWalkieMB = ConfigSettings.WalkieTermMB.Value;
        internal static bool walkieEnum = false;

        public WalkieTerm(string useWalkieKey)
        {
            UseWalkieKey = useWalkieKey;
        }

        public static WalkieTalkie GetWalkie(out WalkieTalkie walkie)
        {
            walkie = null;

            for (int i = 0; i < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; i++)
            {
                if (GameNetworkManager.Instance.localPlayerController.ItemSlots[i] is WalkieTalkie)
                {
                    walkie = GameNetworkManager.Instance.localPlayerController.ItemSlots[i] as WalkieTalkie;
                    break;
                }
            }

            return walkie;
        }

        public static Key GetUseWalkieKey()
        {
            if (Enum.TryParse(UseWalkieKey, out Key keyFromString))
            {
                return keyFromString;
            }
            else
            {
                return Key.LeftAlt;
            }
        }

        public static string GetUseWalkieMouseButton()
        {
            for (int i = 0; i < Enum.GetValues(typeof(MouseButton)).Length; i++)
            {
                MouseButton mb = (MouseButton)i;
                string thisbutton = mb.ToString();

                if (UseWalkieMB == thisbutton)
                {
                    thisbutton = thisbutton.Replace("MouseButton.", "").ToLower();
                    thisbutton += "Button";
                    //Plugin.Log.LogInfo(thisbutton);
                    return thisbutton;
                }
            }
            string defbutton = "leftButton";
            return defbutton;
        }

        internal static bool ActivateWalkie()
        {
            Key walkieKey = GetUseWalkieKey();
            string walkieMouseButton = GetUseWalkieMouseButton();
            if (Keyboard.current[walkieKey].isPressed || Mouse.current[walkieMouseButton].IsActuated())
                return true;
            else
                return false;
        }

        internal static void WalkieTerminal()
        {
            GetWalkie(out WalkieTalkie getmywalkie);

            if (getmywalkie == null)
                return;

            if (!getmywalkie.isBeingUsed)
                return;

            if (ActivateWalkie())
            {
                getmywalkie.UseItemOnClient(true);
                Plugin.MoreLogs("Start Using Walkie Talkie");
                Plugin.instance.Terminal.StartCoroutine(WalkieBeingUsed(getmywalkie));
            }
        }

        internal static IEnumerator WalkieBeingUsed(WalkieTalkie getmywalkie)
        {
            if (walkieEnum)
                yield break;

            WaitForSeconds wait = new(0.15f);

            walkieEnum = true;

            while (ActivateWalkie())
            {
                yield return wait;
            }

            getmywalkie.UseItemOnClient(false);
            Plugin.MoreLogs("ending walkie use");
            walkieEnum = false;
        }
    }
}
