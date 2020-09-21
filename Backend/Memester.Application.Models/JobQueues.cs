namespace Memester.Application.Model
{
    public class JobQueues
    {
        public const string Default = "default";
        public const string ThreadIndexing = "a_thread_indexing";
        public const string BoardIndexing = "b_board_indexing";
        public const string DiskCleanup = "c_diskcleanup";

        public static string[] All => new[]
        {
            Default, ThreadIndexing, BoardIndexing, DiskCleanup
        };
    }
}