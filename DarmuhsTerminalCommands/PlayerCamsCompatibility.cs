using UnityEngine;

namespace TerminalStuff
{
    internal class PlayerCamsCompatibility
    {
        internal static Texture PlayerCamTexture()
        {
            Plugin.MoreLogs("Setting camstexture to monitor, both Solo's body cam and Rick Arg's Helmet cam use the same texture");
            Texture MonitorTexture = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture;
            return MonitorTexture;
        }
    }
}
