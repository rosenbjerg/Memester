namespace Memester.Application.Model
{
    public class JobQueues
    {
        public const string Default = "default";

        public static string[] All => new[]
        {
            Default
        };
    }
}