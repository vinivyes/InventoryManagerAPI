using InventoryManagerAPI.Context;
using InventoryManagerAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

public class TestInventoryContext : InventoryContext
{
    public TestInventoryContext(DbContextOptions<InventoryContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(ConfigureRole);
    }

    //Allow the use of string arrays in the memory database for testing purposes
    private void ConfigureRole(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.allowedActions)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<string[]>(v)
            );

        builder.Property(r => r.notAllowedActions)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<string[]>(v)
            );
    }
}
