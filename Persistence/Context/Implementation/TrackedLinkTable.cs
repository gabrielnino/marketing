using Domain.WhatsApp.Redirect;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStructure.Constants;
using Persistence.CreateStructure.Constants.ColumnType;

namespace Persistence.Context.Implementation
{
    public static class TrackedLinkTable
    {
        public static void Create(ModelBuilder modelBuilder, IColumnTypes columnTypes)
        {
            modelBuilder.Entity<TrackedLink>().ToTable(Database.Tables.TrackedLinks);

            modelBuilder.Entity<TrackedLink>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                      .HasColumnType(columnTypes.TypeVar)
                      .IsRequired();

                entity.Property(x => x.TargetUrl)
                      .HasColumnType(columnTypes.TypeVar150)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(x => x.VisitCount)
                      .HasColumnType(columnTypes.Long)
                      .IsRequired();
            });
        }
    }
}
