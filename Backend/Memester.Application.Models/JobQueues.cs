namespace Memester.Application.Model
{
    public class JobQueues
    {
        public const string Default = "default";
        public const string LoginEmails = "a_login_emails";
        public const string ThreadIndexing = "b_thread_indexing";
        public const string BoardIndexing = "c_board_indexing";
        public const string DiskCleanup = "d_diskcleanup";

        public static string[] All => new[]
        {
            Default, LoginEmails, ThreadIndexing, BoardIndexing, DiskCleanup
        };
    }
}