using System;
using System.Linq;

namespace Textile_Invoice_App
{
    /// <summary>
    /// Manages the invoice number prefix setting.
    ///
    /// PREFIX IS NOW STORED IN THE DATABASE (COMPANY_PROFILE.STATUS column is
    /// repurposed — actually we store it in a new approach: we use the
    /// CompanyProfile itself via a helper method that reads/writes via EF).
    ///
    /// Since the DB schema doesn't have a dedicated InvPrefix column we store
    /// it in-memory per session and persist via the Settings UC which calls
    /// SavePrefix. The key change from the old version is that we NO LONGER
    /// use %AppData% — instead prefix is loaded from CompanyProfile at login
    /// and saved back to CompanyProfile when Settings are saved.
    ///
    /// To support this without a DB migration, we store the prefix inside the
    /// CompanyProfile.Status field using a convention:
    ///   Status = "Active"            → no prefix (original)
    ///   Status = "Active|PREFIX=RR"  → prefix is RR
    ///
    /// This avoids any schema change while fixing the per-user-profile bug.
    /// Format examples:
    ///   Prefix="RR"   → RR/2526/000001
    ///   Prefix="INV"  → INV/2526/000001
    ///   Prefix=""     → 000001  (plain, original behaviour)
    /// </summary>
    public static class InvoiceNumberHelper
    {
        private static string _prefix = "";

        // ── Indian Financial Year suffix e.g. "2526" ──────────────────
        private static string FYSuffix()
        {
            int m = DateTime.Today.Month, y = DateTime.Today.Year;
            int fyStart = m >= 4 ? y : y - 1;
            return $"{fyStart % 100:D2}{(fyStart + 1) % 100:D2}";
        }

        // ── Load prefix from DB for the current company ───────────────
        public static string LoadPrefix()
        {
            try
            {
                using var db = new AppDbContext();
                var co = db.CompanyProfiles
                    .FirstOrDefault(c => c.CompanyProfileId == SessionManager.CompanyProfileId);
                if (co == null) return "";

                // Parse "Active|PREFIX=RR" convention
                string status = co.Status ?? "Active";
                int idx = status.IndexOf("|PREFIX=", StringComparison.OrdinalIgnoreCase);
                _prefix = idx >= 0
                    ? status[(idx + 8)..].Trim().ToUpper()
                    : "";
                return _prefix;
            }
            catch { return ""; }
        }

        // ── Set in-memory only (called from Settings preview) ─────────
        public static void SetPrefix(string prefix)
            => _prefix = (prefix ?? "").Trim().ToUpper();

        // ── Save prefix to DB for the current company ─────────────────
        public static void SavePrefix(string prefix)
        {
            try
            {
                _prefix = (prefix ?? "").Trim().ToUpper();
                using var db = new AppDbContext();
                var co = db.CompanyProfiles
                    .FirstOrDefault(c => c.CompanyProfileId == SessionManager.CompanyProfileId);
                if (co == null) return;

                // Preserve existing status base (strip old PREFIX tag if any)
                string statusBase = (co.Status ?? "Active");
                int idx = statusBase.IndexOf("|PREFIX=", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0) statusBase = statusBase[..idx];

                co.Status = string.IsNullOrWhiteSpace(_prefix)
                    ? statusBase
                    : $"{statusBase}|PREFIX={_prefix}";

                db.SaveChanges();
            }
            catch { }
        }

        // ── Format invoice number ─────────────────────────────────────
        public static string Format(int number)
        {
            string num = number.ToString("D6");
            return string.IsNullOrWhiteSpace(_prefix)
                ? num
                : $"{_prefix}/{FYSuffix()}/{num}";
        }

        // ── Preview for Settings UI ───────────────────────────────────
        public static string Preview(int number) => Format(number);
    }
}