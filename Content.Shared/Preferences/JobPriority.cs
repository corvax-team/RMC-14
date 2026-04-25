
namespace Content.Shared.Preferences
{
    public enum JobPriority
    {
        // These enum values HAVE to match the ones in DbJobPriority in Content.Server.Database
        Never = 0,
        Second = 1,
        SecondFallback = 2,
        First = 3
    }
}

// # CCM priority rework
