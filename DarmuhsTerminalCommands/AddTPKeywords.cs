using static TerminalStuff.AllMyTerminalPatches;
using static UnityEngine.Object;

namespace TerminalStuff
{
    internal class AddTPKeywords
    {
        internal static void CheckForTP()
        {
            //Add TP keywords AFTER they have been purchased and exist
            ShipTeleporter[] objectsOfType = FindObjectsOfType<ShipTeleporter>();

            if (!TerminalStartPatch.doesTPexist && ConfigSettings.terminalTP.Value)
            {
                ShipTeleporter tp = (ShipTeleporter)null;

                foreach (ShipTeleporter tpobject in objectsOfType)
                {
                    if (!tpobject.isInverseTeleporter)
                    {
                        tp = tpobject;
                        break;
                    }
                }

                if (tp != null)
                {
                    TerminalStartPatch.doesTPexist = true;
                    TerminalEvents.AddTeleportKeywords();
                }
                else
                {
                    Plugin.MoreLogs("TP does not exist yet");
                }
            }
            if (!TerminalStartPatch.doesITPexist && ConfigSettings.terminalITP.Value)
            {
                ShipTeleporter itp = (ShipTeleporter)null;
                foreach (ShipTeleporter tpobject in objectsOfType)
                {
                    if (tpobject.isInverseTeleporter)
                    {
                        itp = tpobject;
                        break;
                    }
                }

                if (itp != null && ConfigSettings.terminalITP.Value)
                {
                    TerminalStartPatch.doesITPexist = true;
                    TerminalEvents.AddInverseTeleportKeywords();
                }
            }
        }
    }
}
