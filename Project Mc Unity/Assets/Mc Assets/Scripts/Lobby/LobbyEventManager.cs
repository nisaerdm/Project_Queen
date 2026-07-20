using System;

public static class LobbyEventManager
{
    // Menü durumlarını tuttuğumuz Enum
    public enum LobbyState
    {
        MainMenu,
        Play,
        Garage,
        Settings,
        Credits
    }

    // Herhangi bir menü değiştiğinde tetiklenecek C# Action'ımız
    public static Action<LobbyState> OnMenuStateChanged;
}