using System;

public static class LobbyEventManager
{
    public enum LobbyState
    {
        MainMenu,
        Play,
        Garage,
        Settings,
        Credits
    }
    public static Action<LobbyState> OnMenuStateChanged;
}