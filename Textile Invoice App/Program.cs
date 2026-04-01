namespace Textile_Invoice_App
{
    internal static class Program
    {
        // ── Application version — update before each release ──────────
        public const string AppVersion = "1.0.0";
        public const string AppName = "Billing Invoice Management";
        public const string BuildDate = "2026";

        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Load saved invoice number prefix
            InvoiceNumberHelper.LoadPrefix();

            // Auto-backup reminder
            UC_Settings.CheckBackupReminder();

            Application.Run(new Login());
        }
    }
}