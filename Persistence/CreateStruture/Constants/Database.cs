namespace Persistence.CreateStructure.Constants
{
    /// <summary>
    /// Represents the Database schema constants.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Contains names of database tables.
        /// </summary>
        public static class Tables
        {
            /// <summary>
            /// Name of the Users table.
            /// </summary>
            public const string Users = "Users";

            /// <summary>
            /// Name of the Invoices table.
            /// </summary>
            public const string Invoices = "Invoices";

            /// <summary>
            /// Name of the Products table.
            /// </summary>
            public const string Products = "Products";

            /// <summary>
            /// Name of the ErrorLogs table.
            /// </summary>
            public const string ErrorLogs = "ErrorLogs";

            /// <summary>
            /// Name of the Profiles table.
            /// </summary>
            public const string Profiles = "Profiles";

            /// <summary>
            /// Name of the Experiences table.
            /// </summary>
            public const string Experiences = "Experiences";

            /// <summary>
            /// Name of the ExperienceRoles table.
            /// </summary>
            public const string ExperienceRoles = "ExperienceRoles";

            /// <summary>
            /// Name of the Educations table.
            /// </summary>
            public const string Educations = "Educations";

            /// <summary>
            /// Name of the Communications table.
            /// </summary>
            public const string Communications = "Communications";
        }

        /// <summary>
        /// Contains names of database indexes.
        /// </summary>
        public static class Index
        {
            /// <summary>
            /// Unique index on the Email column in the Users table.
            /// </summary>
            public const string IndexEmail = "UC_Users_Email";

            /// <summary>
            /// Index on Profile.FullName.
            /// </summary>
            public const string IndexProfileFullName = "IX_Profiles_FullName";

            /// <summary>
            /// Index on Profile.Url.
            /// </summary>
            public const string IndexProfileUrl = "UC_Profiles_Url";

            /// <summary>
            /// Index on Experiences by Profile.
            /// </summary>
            public const string IndexExperienceByProfile = "IX_Experiences_ProfileId";

            /// <summary>
            /// Index on ExperienceRoles by Experience.
            /// </summary>
            public const string IndexRoleByExperience = "IX_ExperienceRoles_ExperienceId";

            /// <summary>
            /// Index on Educations by Profile.
            /// </summary>
            public const string IndexEducationByProfile = "IX_Educations_ProfileId";

            /// <summary>
            /// Index on Communications by Profile.
            /// </summary>
            public const string IndexCommunicationByProfile = "IX_Communications_ProfileId";
        }
    }
}
