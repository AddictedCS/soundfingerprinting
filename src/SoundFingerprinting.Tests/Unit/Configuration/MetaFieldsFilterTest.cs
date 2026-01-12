namespace SoundFingerprinting.Tests.Unit.Configuration
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using SoundFingerprinting.Configuration;

    [TestFixture]
    public class MetaFieldsFilterTest
    {
        private readonly IMetaFieldsFilter metadataFilter = new MetaFieldsFilter();

        [Test]
        public void EmptyFiltersShouldPass()
        {
            var metaFields = new Dictionary<string, string>();
            var yesFilters = new Dictionary<string, string>();
            var noFilters = new Dictionary<string, string>();

			Assert.That(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters), Is.True);
        }
        
        [Test]
        public void ShouldApplyPositiveFilterOnEmptyMetaFields()
        {
            var metaFields = new Dictionary<string, string>();
            var yesFilters = new Dictionary<string, string>{{"COUNTRY", "US"}};
            var noFilters = new Dictionary<string, string>();

			Assert.That(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters), Is.False);
        }
        
        [Test]
        public void ShouldApplyNegativeFilterOnEmptyMetaFields()
        {
            var metaFields = new Dictionary<string, string>();
            var yesFilters = new Dictionary<string, string>();
            var noFilters = new Dictionary<string, string>{{"COUNTRY", "US"}};

			Assert.That(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters), Is.True);
        }
        
        [Test]
        public void ShouldApplyPositiveFilter()
        {
            var metaFields = new Dictionary<string, string> {{"COUNTRY", "US"}};
            var yesFilters = new Dictionary<string, string> {{"COUNTRY", "US"}};
            var noFilters = new Dictionary<string, string>();

			Assert.That(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters), Is.True);
        }
        
        [Test]
        public void ShouldApplyPositiveFilter2()
        {
            var metaFields = new Dictionary<string, string> {{"COUNTRY", "US"}};
            var yesFilters = new Dictionary<string, string> {{"COUNTRY", "NZ"}};
            var noFilters = new Dictionary<string, string>();

			Assert.That(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters), Is.False);
        }
        
        [Test]
        public void ShouldApplyNegativeFilter()
        {
            var metaFields = new Dictionary<string, string> {{"COUNTRY", "US"}};
            var yesFilters = new Dictionary<string, string>();
            var noFilters = new Dictionary<string, string>{{"COUNTRY", "US"}};

			Assert.That(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters), Is.False);
        }
    }
}