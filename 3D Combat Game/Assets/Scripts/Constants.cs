namespace Assets.Scripts.Constants
{
    public static class Objects
    {
        public static string GameSettings => "GameSettings";
        public static string GeneralLogic => "GeneralLogic";
        public static string SpawnableItems => "SpawnableItems";
        public static string BlueTeam => "BlueTeamSpawn";
        public static string RedTeam => "RedTeamSpawn";
    }

    public static class HUD
    {
        public static string RedPoints => "RedTeamPoints";
        public static string BluePoints => "BlueTeamPoints";
    }

    public static class Audio
    {
        public static string AreaSecured => "09._area_secured";
        public static string AreaLost => "10._area_lost";
    }

    public static class Scenes
    {
        public static string MainMenu => "MainMenu";
        public static string ConqustGameMode => "SampleScene";
    }

    public static class MainMenu
    {
        // Menu Screens
        public static string MainScreen => "MainMenu";
        public static string Controls => "ControlsMenu";
        public static string Credits => "CreditsMenu";
        public static string ConfigureGame => "ConfigureGameMenu";

        // Drop Down Fields
        public static string RedTeamDropDown => "RedTeamBotType";
        public static string BlueTeamDropDown => "BlueTeamBotType";
        public static string PlayerTeamDropDown => "PlayersTeam";
    }
}
