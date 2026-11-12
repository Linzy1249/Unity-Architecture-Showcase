namespace UnityArchitectureShowcase.Data
{
    public class AppData
    {
        public static AppData instance { get; } = new AppData();

        public ConfigData configData = new ConfigData();
    }
}