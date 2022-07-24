namespace TelegramTools.Uploader.Stats;

public interface IStatsService
{
    void AddStat(long? sizeBytes, StatType type);
    void AddStat(StatType type);
    StatResult GetStats();
}