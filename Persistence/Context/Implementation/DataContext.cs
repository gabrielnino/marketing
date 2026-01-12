using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;

namespace Persistence.Context.Implementation
{
    /// <summary>
    /// Represents a DataContentext
    /// </summary>
    /// <param name="options">The options to be used by DbContext</param>
    /// <param name="columnTypes">The column types used for the database</param>
    public class DataContext(DbContextOptions options, IColumnTypes columnTypes) : DbContext(options), IDataContext
    {
        protected readonly IColumnTypes _columnTypes = columnTypes;

        /// <summary>
        ///  Initializes the data context. This typically includes opening connections, applying migrations, creating
        ///  the database if it does not exist, and seeding any required initial data.
        /// </summary>
        /// <returns>
        /// <c>true</c> if initialization succeeded; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Initialize()
        {
            try
            {
                Database.Migrate();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while initializing the database:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }


        /// <summary>
        /// Configures the EF Core model for this context.
        /// Sets up tables, keys, indexes, and column mappings using the provided <see cref="IColumnTypes"/>.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Core domain ===
            //ProfileTable.Create(modelBuilder, _columnTypes);
            //ExperienceTable.Create(modelBuilder, _columnTypes);
            //ExperienceRoleTable.Create(modelBuilder, _columnTypes);
            //EducationTable.Create(modelBuilder, _columnTypes);
            //CommunicationTable.Create(modelBuilder, _columnTypes);
            ErrorLogTable.Create(modelBuilder, _columnTypes);
            TrackedLinkTable.Create(modelBuilder, _columnTypes);
            // DB function mapping
            modelBuilder
                .HasDbFunction(typeof(DataContext)
                    .GetMethod(nameof(StringCompareOrdinal), new[] { typeof(string), typeof(string) })!)
                .HasName("StringCompareOrdinal");
        }


        public static int StringCompareOrdinal(string a, string b) => throw new NotSupportedException();
    }
}
