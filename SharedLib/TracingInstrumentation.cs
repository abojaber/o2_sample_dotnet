using System.Diagnostics;

namespace SharedLib
{
    public static class TracingInstrumentation
    {
        public static readonly ActivitySource ActivitySource = new("SharedLib");

        public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
        {
            return ActivitySource.StartActivity(name, kind);
        }
    }
}