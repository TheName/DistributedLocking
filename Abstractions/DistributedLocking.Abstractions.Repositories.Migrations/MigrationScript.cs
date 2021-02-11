using System;

namespace DistributedLocking.Abstractions.Repositories.Migrations
{
    /// <summary>
    /// A migration script value object.
    /// </summary>
    public class MigrationScript
    {
        /// <summary>
        /// Migration script's name.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Migration script's content.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="name">
        /// Migration script's name.
        /// </param>
        /// <param name="content">
        /// Migration script's content.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> or <paramref name="content"/> is null or whitespace.
        /// </exception>
        public MigrationScript(string name, string content)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{nameof(name)} cannot be null or whitespace!");
            }
            
            Name = name;
            
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException($"{nameof(content)} cannot be null or whitespace!");
            }
            Content = content;
        }

        #region Operators

        /// <summary>
        /// The equality operator.
        /// </summary>
        /// <param name="migrationScript">
        /// The <see cref="MigrationScript"/>.
        /// </param>
        /// <param name="otherMigrationScript">
        /// The <see cref="MigrationScript"/>.
        /// </param>
        /// <returns>
        /// True if <paramref name="migrationScript"/> and <paramref name="otherMigrationScript"/> are equal, false otherwise.
        /// </returns>
        public static bool operator ==(MigrationScript migrationScript, MigrationScript otherMigrationScript) =>
            Equals(migrationScript, otherMigrationScript);

        /// <summary>
        /// The inequality operator.
        /// </summary>
        /// <param name="migrationScript">
        /// The <see cref="MigrationScript"/>.
        /// </param>
        /// <param name="otherMigrationScript">
        /// The <see cref="MigrationScript"/>.
        /// </param>
        /// <returns>
        /// True if <paramref name="migrationScript"/> and <paramref name="otherMigrationScript"/> are not equal, false otherwise.
        /// </returns>
        public static bool operator !=(MigrationScript migrationScript, MigrationScript otherMigrationScript) =>
            !(migrationScript == otherMigrationScript);

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj) =>
            obj is MigrationScript other &&
            other.GetHashCode() == GetHashCode();

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + Name.GetHashCode();
                hash = hash * 31 + Content.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}