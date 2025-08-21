using GK.Talks.Infrastructure.Services;
using GK.Talks.Tests.Shared;

namespace GK.Talks.Tests.Unit.Core;
[TestFixture]
public class RegistrationFeeCalculatorTests : TestBase
{
    private RegistrationFeeCalculator _calculator = default!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _calculator = new RegistrationFeeCalculator();
    }

    [TestCase(0, 500)]
    [TestCase(1, 500)]
    [TestCase(2, 250)]
    [TestCase(3, 250)]
    [TestCase(4, 100)]
    [TestCase(5, 100)]
    [TestCase(6, 50)]
    [TestCase(9, 50)]
    [TestCase(10, 0)]
    [TestCase(20, 0)]
    public void CalculateFee_ExperienceBoundaries_ReturnsExpected(int years, int expected)
    {
        int actual = _calculator.CalculateFee(years);
        Assert.That(actual, Is.EqualTo(expected));
    }
}