using GK.Talks.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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