using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFrameworkAuditTrail
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlite("Data Source=databases/Application.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ItemTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AuditTypeConfiguration());
        }

        public DbSet<Item> Item { get; set; }
        internal DbSet<Audit> Audits { get; set; }


        public override int SaveChanges()
        {
            var audits = new List<Audit>();
            var modifiedEntities = ChangeTracker.Entries();

            foreach (var change in modifiedEntities)
            {
                var entityType = change.Entity.GetType().Name;
                if (entityType == "Audit")
                    continue;

                if (change.State == EntityState.Modified)
                {
                    foreach (var prop in change.OriginalValues.Properties)
                    {
                        var id = change.CurrentValues["Id"].ToString();

                        //here both originalValue and currentValue  are same and it's newly updated value 
                        var originalValue = change.OriginalValues[prop]?.ToString();
                        var currentValue = change.CurrentValues[prop]?.ToString();
                        if (originalValue != currentValue)
                        {
                            audits.Add(
                                new Audit
                                {
                                    TableName = entityType,
                                    FieldName = prop.Name,
                                    UserName = System.Environment.UserName,
                                    Action = Action.U,
                                    OriginalValue = originalValue,
                                    NewValue = currentValue,
                                    Log = $"Edited item named {prop.Name} in {entityType} Id {id}.",
                                }
                            );
                        }
                    }
                }
                else if (change.State == EntityState.Added)
                {
                    foreach (var prop in change.OriginalValues.Properties)
                    {
                        var id = change.CurrentValues["Id"].ToString();

                        //here both originalValue and currentValue  are same and it's newly updated value 
                        // var originalValue = change.OriginalValues[prop]?.ToString();
                        var currentValue = change.CurrentValues[prop]?.ToString();
                        // if (originalValue != currentValue)
                        // {
                            audits.Add(
                                new Audit
                                {
                                    TableName = entityType,
                                    FieldName = prop.Name,
                                    UserName = System.Environment.UserName,
                                    Action = Action.U,
                                    OriginalValue = "< New >",
                                    NewValue = currentValue,
                                    Log = $"New item named {prop.Name} in {entityType} Id {id}.",
                                }
                            );
                        // }
                    }
                }
            }

            Audits.AddRange(audits);
            return base.SaveChanges();
        }
    }
    public class ItemTypeConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("Item");
        }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    internal class AuditTypeConfiguration : IEntityTypeConfiguration<Audit>
    {
        public void Configure(EntityTypeBuilder<Audit> builder)
        {
            builder.HasKey(e => e.Id);

            builder.ToTable("Audit");
        }
    }

    internal class Audit
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string UserName { get; set; }
        public Action Action { get; set; }
        public string OriginalValue { get; set; }
        public string NewValue { get; set; }
        public string Log { get; set; }
    }

    public enum Action
    {
        I, U, D
    }
}