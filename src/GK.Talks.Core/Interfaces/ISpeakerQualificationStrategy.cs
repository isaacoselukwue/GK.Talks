using GK.Talks.Core.Aggregates.SpeakerAggregate;

namespace GK.Talks.Core.Interfaces;
public interface ISpeakerQualificationStrategy
{
    bool IsQualified(Speaker speaker);
}