namespace GK.Talks.Core.Aggregates.SpeakerAggregate;
public class Session
{
    public int Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public bool Approved { get; private set; }

    private Session()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Session(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Session title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Session description is required.", nameof(description));
        Title = title;
        Description = description;
        Approved = false;
    }

    internal void Approve() => Approved = true;
}