using System;
using TestHelpers.Attributes;
using Xunit;

namespace DistributedLocking.Abstractions.Repositories.Migrations.UnitTests
{
    public class MigrationScript_Should
    {
        [Theory]
        [AutoMoqWithInlineData("")]
        [AutoMoqWithInlineData((string)null)]
        [AutoMoqWithInlineData(" ")]
        [AutoMoqWithInlineData("\n")]
        public void Throw_When_TryingToCreateWithNullOrWhitespaceName(
            string name,
            string content)
        {
            Assert.Throws<ArgumentException>(() => new MigrationScript(name, content));
        }
        
        [Theory]
        [AutoMoqWithInlineData("")]
        [AutoMoqWithInlineData((string)null)]
        [AutoMoqWithInlineData(" ")]
        [AutoMoqWithInlineData("\n")]
        public void Throw_When_TryingToCreateWithNullOrWhitespaceContent(
            string content,
            string name)
        {
            Assert.Throws<ArgumentException>(() => new MigrationScript(name, content));
        }

        [Theory]
        [AutoMoqData]
        public void NotThrow_When_TryingToCreateWithNonNullOrWhitespaceNameAndContent(string name, string content)
        {
            _ = new MigrationScript(name, content);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnTrue_When_ComparingDifferentObjectsWithSameValues(string name, string content)
        {
            var script1 = new MigrationScript(name, content);
            var script2 = new MigrationScript(name, content);
            
            Assert.Equal(script1, script2);
            Assert.True(script1 == script2);
            Assert.False(script1 != script2);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnFalse_When_ComparingDifferentObjectsWithDifferentName(
            string name,
            string otherName,
            string content)
        {
            var script1 = new MigrationScript(name, content);
            var script2 = new MigrationScript(otherName, content);
            
            Assert.NotEqual(script1, script2);
            Assert.False(script1 == script2);
            Assert.True(script1 != script2);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnFalse_When_ComparingDifferentObjectsWithDifferentContent(
            string name,
            string content,
            string otherContent)
        {
            var script1 = new MigrationScript(name, content);
            var script2 = new MigrationScript(name, otherContent);
            
            Assert.NotEqual(script1, script2);
            Assert.False(script1 == script2);
            Assert.True(script1 != script2);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnName_When_CallingToString(MigrationScript migrationScript)
        {
            Assert.Equal(migrationScript.Name, migrationScript.ToString());
        }
    }
}