namespace proxy.config;

public record ProxyOptions(int Port, string MonolithUrl, string MoviesServiceUrl, string EventsServiceUrl, bool GradualMigration, int MoviesMigrationPercent)
{
    public static ProxyOptions FromEnvironment() => new(
        Port: ReadInt("PORT", 8000),
        MonolithUrl: Read("MONOLITH_URL", "http://monolith:8080"),
        MoviesServiceUrl: Read("MOVIES_SERVICE_URL", "http://movies-service:8081"),
        EventsServiceUrl: Read("EVENTS_SERVICE_URL", "http://events-service:8082"),
        GradualMigration: bool.TryParse(Environment.GetEnvironmentVariable("GRADUAL_MIGRATION"), out var flag) && flag,
        MoviesMigrationPercent: Math.Clamp(ReadInt("MOVIES_MIGRATION_PERCENT", 0), 0, 100));

    public bool RouteToMoviesMicroservice()
    {
        bool res = GradualMigration;
        if (res)
            res = Random.Shared.Next(100) < MoviesMigrationPercent;
        return res;
    }

    private static string Read(string name, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
    }

    private static int ReadInt(string name, int defaultValue)
    {
        return int.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;
    }
}