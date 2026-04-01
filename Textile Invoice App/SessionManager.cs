namespace Textile_Invoice_App
{
    /// <summary>
    /// Populated once at login. Every UC reads CompanyProfileId from here.
    /// </summary>
    public static class SessionManager
    {
        public static int    UserId           { get; set; }
        public static string Username         { get; set; } = "";
        public static string FullName         { get; set; } = "";
        public static int    CompanyProfileId { get; set; }
        public static string CompanyName      { get; set; } = "";

        public static void Clear()
        {
            UserId = 0; Username = ""; FullName = "";
            CompanyProfileId = 0; CompanyName = "";
        }
    }
}
