using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using GameNetcodeStuff;
using System;
using System.Net.Mail;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

namespace TerminalStuff
{
    public class walkieTerm : MonoBehaviour
    {
        //static PlayerControllerB getmyself = GameNetworkManager.Instance.localPlayerController;

        public static string UseWalkieKey = ConfigSettings.walkieTermKey.Value;
        public static string UseWalkieMB = ConfigSettings.walkieTermMB.Value;

        public walkieTerm(string useWalkieKey)
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
            for(int i = 0; i < Enum.GetValues(typeof(MouseButton)).Length; i++)
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
            GrabbableObject result;
            GrabbableObject getmywalkie = getWalkie(out result);

            bool usingWalkFromTerm = false;

            if (getmywalkie != null)
            {
                while (tinstance.terminalInUse && ConfigSettings.walkieTerm.Value)
                {
                    if (activateWalkie() && !usingWalkFromTerm)
                    {
                        getmywalkie.UseItemOnClient(true);
                        usingWalkFromTerm = true;
                        Plugin.Log.LogInfo("push to use walkie key was pressed");
                        yield return new WaitForSeconds(0.2f);
                    }
                    else if(!activateWalkie() && usingWalkFromTerm)
                    {
                        
                        Plugin.Log.LogInfo("ending walkie use");
                        usingWalkFromTerm = false;
                        getmywalkie.UseItemOnClient(false);
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                        yield return new WaitForSeconds(0.2f);

                }
            }
            else
                Plugin.Log.LogInfo("no walkie found in inventory");


            if(!tinstance.terminalInUse)
                Plugin.Log.LogInfo("out of terminal");
            yield break;
        }
    }
}
