using Microsoft.EntityFrameworkCore;
using Uber.DAL.DataBase;
using Xunit;

namespace Uber.Behavior.Tests;

public sealed class PersistenceModelTests
{
    [Fact]
    public void Sql_server_schema_matches_pre_extraction_baseline()
    {
        var options = new DbContextOptionsBuilder<UberDBContext>()
            .UseSqlServer("Server=unused;Database=SchemaComparison;Integrated Security=true;TrustServerCertificate=true")
            .Options;
        using var context = new UberDBContext(options);
        var actual = context.Database.GenerateCreateScript();
        var expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory,
            "Baselines", "schema-before-domain.sql"));
        Assert.Equal(expected.ReplaceLineEndings("\n"), actual.ReplaceLineEndings("\n"));
    }
}
