using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants;

namespace Persistence.Context.Implementation
{
    public static class ErrorLogTable
    {
        public static void Create(ModelBuilder modelBuilder, IColumnTypes columnTypes)
        {
            modelBuilder.Entity<ErrorLog>().ToTable(Database.Tables.ErrorLogs);
            modelBuilder.Entity<ErrorLog>(entity =>
            {
                // Primary key
                entity.Property(i => i.Id)
                  .HasColumnType(columnTypes.TypeVar)
                  .IsRequired();
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Timestamp)
                          .HasColumnType(columnTypes.TypeTime)
                          .IsRequired();


                entity.Property(i => i.Level)
                        .HasColumnType(columnTypes.TypeVar150)
                        .HasMaxLength(150)
                        .IsRequired();

                entity.Property(i => i.Message)
                        .HasColumnType(columnTypes.TypeVar150)
                        .HasMaxLength(150)
                        .IsRequired();

                entity.Property(i => i.ExceptionType)
                        .HasColumnType(columnTypes.TypeVar150)
                        .HasMaxLength(150)
                        .IsRequired();

                entity.Property(i => i.StackTrace)
                        .HasColumnType(columnTypes.TypeVar150)
                        .HasMaxLength(150)
                        .IsRequired();

                entity.Property(i => i.Context)
                        .HasColumnType(columnTypes.TypeVar150)
                        .HasMaxLength(150)
                        .IsRequired(); ;
            });
        }
    }
}
