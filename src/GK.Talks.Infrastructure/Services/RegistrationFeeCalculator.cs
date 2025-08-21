using GK.Talks.Core.Interfaces;
namespace GK.Talks.Infrastructure.Services;
public class RegistrationFeeCalculator : IRegistrationFeeCalculator
{
    public int CalculateFee(int experienceInYears) => experienceInYears switch
    {
        <= 1 => 500,
        >= 2 and <= 3 => 250,
        >= 4 and <= 5 => 100,
        >= 6 and <= 9 => 50,
        _ => 0
    };
}