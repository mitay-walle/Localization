namespace mitaywalle.Plugins.Localization
{
    public interface ILocalized
    {
        bool enabled { get; set; }
        string Localize(bool logs = true);
    }
}
