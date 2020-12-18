using AutoFixture.Xunit2;

namespace TestHelpers.Attributes
{
    public class AutoMoqWithInlineDataAttribute : InlineAutoDataAttribute
    {
        public AutoMoqWithInlineDataAttribute(params object[] inlineData) : base(new AutoMoqDataAttribute(), inlineData)
        {
        }
    }
}