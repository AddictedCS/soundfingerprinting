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
            
            Assert.IsTrue(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters));
        }
        
        [Test]
        public void ShouldApplyPositiveFilterOnEmptyMetaFields()
        {
            var metaFields = new Dictionary<string, string>();
            var yesFilters = new Dictionary<string, string>{{"COUNTRY", "US"}};
            var noFilters = new Dictionary<string, string>();
            
            Assert.IsFalse(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters));
        }
        
        [Test]
        public void ShouldApplyNegativeFilterOnEmptyMetaFields()
        {
            var metaFields = new Dictionary<string, string>();
            var yesFilters = new Dictionary<string, string>();
            var noFilters = new Dictionary<string, string>{{"COUNTRY", "US"}};
            
            Assert.IsTrue(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters));
        }
        
        [Test]
        public void ShouldApplyPositiveFilter()
        {
            var metaFields = new Dictionary<string, string> {{"COUNTRY", "US"}};
            var yesFilters = new Dictionary<string, string> {{"COUNTRY", "US"}};
            var noFilters = new Dictionary<string, string>();

            Assert.IsTrue(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters));
        }
        
        [Test]
        public void ShouldApplyPositiveFilter2()
        {
            var metaFields = new Dictionary<string, string> {{"COUNTRY", "US"}};
            var yesFilters = new Dictionary<string, string> {{"COUNTRY", "US"}, {"COUNTRY", "NZ"}};
            var noFilters = new Dictionary<string, string>();

            Assert.IsTrue(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters));
        }
        
        [Test]
        public void ShouldApplyNegativeFilter()
        {
            var metaFields = new Dictionary<string, string> {{"COUNTRY", "US"}};
            var yesFilters = new Dictionary<string, string>();
            var noFilters = new Dictionary<string, string>{{"COUNTRY", "US"}};

            Assert.IsFalse(metadataFilter.PassesFilters(metaFields, yesFilters, noFilters));
        }
    }
}