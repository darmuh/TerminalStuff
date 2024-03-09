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

        public static string UseWalkieKey = ConfigSettings.walkieTermKey.Value;
        public static string UseWalkieMB = ConfigSettings.walkieTermMB.Value;

        public WalkieTerm(string useWalkieKey)
        {
            UseWalkieKey = useWalkieKey;
        }

        public static GrabbableObject getWalkie(out GrabbableObject walkie)
        {
            walkie = null;

            for (int i = 0; i < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; i++)
            {
                if (GameNetworkManager.Instance.localPlayerController.ItemSlots[i] is WalkieTalkie)
                {
                    walkie = GameNetworkManager.Instance.localPlayerController.ItemSlots[i];
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

        private static bool activateWalkie()
        {
            Key walkieKey = GetUseWalkieKey();
            string walkieMouseButton = GetUseWalkieMouseButton();
            if (Keyboard.current[walkieKey].isPressed || Mouse.current[walkieMouseButton].IsActuated())
                return true;
            else
                return false;
        }

        public static IEnumerator TalkinTerm(Terminal tinstance)
        {
            GrabbableObject getmywalkie = getWalkie(out getmywalkie);

            bool usingWalkFromTerm = false;

            if (getmywalkie != null)
            {
                while (tinstance.terminalInUse && ConfigSettings.walkieTerm.Value)
                {
                    if (activateWalkie() && !usingWalkFromTerm)
                    {
                        getmywalkie.UseItemOnClient(true);
                        usingWalkFromTerm = true;
                        Plugin.MoreLogs("push to use walkie key was pressed");
                        yield return new WaitForSeconds(0.2f);
                    }
                    else if (!activateWalkie() && usingWalkFromTerm)
                    {

                        Plugin.MoreLogs("ending walkie use");
                        usingWalkFromTerm = false;
                        getmywalkie.UseItemOnClient(false);
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                        yield return new WaitForSeconds(0.2f);

                }
            }
            else
                Plugin.MoreLogs("no walkie found in inventory");


            if (!tinstance.terminalInUse)
                Plugin.MoreLogs("out of terminal");
            yield break;
        }
    }
}
