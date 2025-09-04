using OpenLib.CoreMethods;

namespace TutorialNameSpace; //change this

internal class Tutorial
{
    internal static void AddMyCommands()
    {
        AddingThings.AddBasicCommand("command1", "command1", "command 1 text", false, true);
    }

    internal static void OnTerminalAwake(Terminal instance)
    {
        //add log message here to show your subscriber has been invoked
        AddMyCommands();
    }

    internal static void Subscribers() //call this method from Plugin.cs
    {
        OpenLib.Events.EventManager.TerminalAwake.AddListener(OnTerminalAwake);
        //add log message here to show your subscriber method has been added to openlib's event
    }
}
