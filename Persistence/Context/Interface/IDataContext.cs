namespace Persistence.Context.Interface
{
    /// <summary>
    /// Defines a contract for be an DataContext.
    /// </summary>
    public interface IDataContext
    {
        /// <summary>
        ///  Initializes the data context. This typically includes opening connections, applying migrations, creating
        ///  the database if it does not exist, and seeding any required initial data.
        /// </summary>
        /// <returns>
        /// <c>true</c> if initialization succeeded; otherwise, <c>false</c>.
        /// </returns>
        bool Initialize();
    }
}
