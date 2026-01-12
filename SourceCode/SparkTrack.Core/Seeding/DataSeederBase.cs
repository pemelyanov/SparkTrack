namespace SparkTrack.Core.Seeding;

public abstract class DataSeederBase : IDataSeeder
{
    private bool m_isSeeded;
    
    public async Task SeedAsync()
    {
        if (m_isSeeded) return;

        m_isSeeded = true;
        await ProcessSeedAsync();
    }

    protected abstract Task ProcessSeedAsync();
}