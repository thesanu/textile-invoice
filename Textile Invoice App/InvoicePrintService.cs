// ============================================================
//  InvoicePrintService.cs  —  Pure .NET Core WinForms
//  PDF via PdfSharpCore  (NuGet: PdfSharpCore 1.3.x)
//  Fixed: watermark overlap, company name truncation,
//         GRAND TOTAL gap, meta/party box clipping,
//         table column widths, row height
// ============================================================
//  NuGet:
//  <PackageReference Include="PdfSharpCore" Version="1.3.65" />
//  <PackageReference Include="Microsoft.Data.SqlClient" Version="5.*" />
// ============================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Textile_Invoice_App
{
    // ════════════════════════════════════════════════════════
    //  DATA MODEL
    // ════════════════════════════════════════════════════════
    public class InvoiceData
    {
        public string CompanyName { get; set; } = "";
        public string CompanyAddress { get; set; } = "";
        public string CompanyGstin { get; set; } = "";
        public string CompanyPan { get; set; } = "";
        public string CompanyPhone { get; set; } = "";

        public string InvoiceNo { get; set; } = "";
        public string InvoiceDate { get; set; } = "";
        public string ChallanNo { get; set; } = "—";
        public string ChallanDate { get; set; } = "—";

        public string ClientName { get; set; } = "";
        public string ClientAddr { get; set; } = "";
        public string ClientGstin { get; set; } = "";
        public string ClientState { get; set; } = "";
        public string Transport { get; set; } = "—";

        public decimal SubTotal { get; set; }
        public decimal CgstPct { get; set; }
        public decimal CgstAmt { get; set; }
        public decimal SgstPct { get; set; }
        public decimal SgstAmt { get; set; }
        public decimal IgstPct { get; set; }
        public decimal IgstAmt { get; set; }
        public decimal Roundup { get; set; }
        public decimal GrandTotal { get; set; }
        public string AmountWords { get; set; } = "";

        public List<InvoiceLineItem> Items { get; set; } = new();
    }

    public class InvoiceLineItem
    {
        public int Sr { get; set; }
        public string Description { get; set; } = "";
        public string HsnCode { get; set; } = "";
        public string PChNo { get; set; } = "";
        public string CoChNo { get; set; } = "";
        public int Pcs { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public string Per { get; set; } = "";
        public decimal Amount { get; set; }
    }

    // ════════════════════════════════════════════════════════
    //  SERVICE
    // ════════════════════════════════════════════════════════
    public static class InvoicePrintService
    {
        // ── Connection string read from AppDbContext — single source of truth ──
        private static string ConnStr
        {
            get
            {
                using var db = new AppDbContext();
                return db.Database.GetConnectionString()!;
            }
        }

        // ── A4 page geometry (points: 1 pt = 1/72 inch) ─────────
        private const double PageW = 595.28;
        private const double PageH = 841.89;
        private const double ML = 30;           // left margin
        private const double MT = 28;           // top margin
        private const double CW = PageW - ML * 2; // content width = 535.28

        // ── Brand colours ────────────────────────────────────────
        private static readonly XColor CBlue = XColor.FromArgb(29, 78, 216);
        private static readonly XColor CBlueLt = XColor.FromArgb(219, 234, 254);
        private static readonly XColor CSlate = XColor.FromArgb(241, 245, 249);
        private static readonly XColor CBorder = XColor.FromArgb(203, 213, 225);
        private static readonly XColor CTextDark = XColor.FromArgb(15, 23, 42);
        private static readonly XColor CTextMid = XColor.FromArgb(30, 41, 59);
        private static readonly XColor CTextMuted = XColor.FromArgb(100, 116, 139);
        private static readonly XColor CGreen = XColor.FromArgb(21, 128, 61);
        private static readonly XColor CRowAlt = XColor.FromArgb(248, 250, 252);

        // ════════════════════════════════════════════════════════
        //  DB LOAD
        // ════════════════════════════════════════════════════════
        public static InvoiceData? LoadFromDb(int invoiceId)
        {
            var d = new InvoiceData();
            try
            {
                using var conn = new SqlConnection(ConnStr);
                conn.Open();

                const string sql = @"
                    SELECT h.INVOICE_NO, h.INVOICE_DATE,
                           h.CHALLAN_NO, h.CHALLAN_DATE,
                           h.TOTAL_AMOUNT, h.CGST_PCT, h.CGST,
                           h.SGST_PCT, h.SGST, h.IGST_PCT, h.IGST,
                           h.ROUNDUP, h.GRAND_TOTAL,
                           cp.COMPANY_NAME,
                           ISNULL(cp.ADDRESS1,'')+', '+ISNULL(cp.CITY,'')+
                               ' - '+ISNULL(cp.PINCODE,'')+', '+ISNULL(cp.STATE,'') AS COMP_ADDR,
                           ISNULL(cp.GSTIN,'')  AS C_GSTIN,
                           ISNULL(cp.PAN,'')    AS C_PAN,
                           ISNULL(cp.PHONE,'')  AS PHONE,
                           ISNULL(cl.ACC_NM,'') AS CLIENT_NM,
                           ISNULL(cl.BILL_ADD1,'')+', '+ISNULL(cl.BILL_CITY,'')+
                               ' - '+ISNULL(cl.BILL_PINCODE,'') AS CLIENT_ADDR,
                           ISNULL(cl.GSTIN,'')      AS CLIENT_GSTIN,
                           ISNULL(cl.BILL_STATE,'') AS CLIENT_STATE,
                           ISNULL(tr.ACC_NM,'—')    AS TRANSPORT_NM
                    FROM   INVOICE_HEADER h
                    JOIN   COMPANY_PROFILE cp ON cp.COMPANY_PROFILE_ID = h.COMPANY_PROFILE_ID
                    LEFT   JOIN ACCOUNTS cl    ON cl.ACCOUNT_ID = h.CLIENT_ID
                    LEFT   JOIN ACCOUNTS tr    ON tr.ACCOUNT_ID = h.TRANSPORT_ID
                    WHERE  h.INVOICE_ID = @id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", invoiceId);
                    using var r = cmd.ExecuteReader();
                    if (!r.Read()) return null;

                    d.CompanyName = S(r, "COMPANY_NAME");
                    d.CompanyAddress = S(r, "COMP_ADDR");
                    d.CompanyGstin = S(r, "C_GSTIN");
                    d.CompanyPan = S(r, "C_PAN");
                    d.CompanyPhone = S(r, "PHONE");
                    d.InvoiceNo = S(r, "INVOICE_NO");
                    d.InvoiceDate = r["INVOICE_DATE"] != DBNull.Value
                        ? Convert.ToDateTime(r["INVOICE_DATE"]).ToString("dd MMM yyyy") : "";
                    d.ChallanNo = NonEmpty(S(r, "CHALLAN_NO"));
                    d.ChallanDate = r["CHALLAN_DATE"] != DBNull.Value
                        ? Convert.ToDateTime(r["CHALLAN_DATE"]).ToString("dd MMM yyyy") : "—";
                    d.ClientName = S(r, "CLIENT_NM");
                    d.ClientAddr = S(r, "CLIENT_ADDR");
                    d.ClientGstin = S(r, "CLIENT_GSTIN");
                    d.ClientState = S(r, "CLIENT_STATE");
                    d.Transport = NonEmpty(S(r, "TRANSPORT_NM"));
                    d.SubTotal = D(r, "TOTAL_AMOUNT");
                    d.CgstPct = D(r, "CGST_PCT"); d.CgstAmt = D(r, "CGST");
                    d.SgstPct = D(r, "SGST_PCT"); d.SgstAmt = D(r, "SGST");
                    d.IgstPct = D(r, "IGST_PCT"); d.IgstAmt = D(r, "IGST");
                    d.Roundup = D(r, "ROUNDUP");
                    d.GrandTotal = D(r, "GRAND_TOTAL");
                }

                const string iSql = @"
                    SELECT ii.ITEM_ID, ii.P_CH_NO, ii.CO_CH_NO, ii.HSN_CODE,
                           ii.PCS, ii.QTY, ii.RATE, ii.PER, ii.AMOUNT,
                           ISNULL(dm.DESIGN_NAME,'—') AS DESIGN_NAME
                    FROM   INVOICE_ITEMS ii
                    LEFT   JOIN DESIGN_MASTER dm ON dm.DESIGN_ID = ii.DESIGN_ID
                    WHERE  ii.INVOICE_ID = @id ORDER BY ii.ITEM_ID";

                using (var cmd = new SqlCommand(iSql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", invoiceId);
                    using var r = cmd.ExecuteReader();
                    int sr = 1;
                    while (r.Read())
                        d.Items.Add(new InvoiceLineItem
                        {
                            Sr = sr++,
                            Description = S(r, "DESIGN_NAME"),
                            HsnCode = S(r, "HSN_CODE"),
                            PChNo = S(r, "P_CH_NO"),
                            CoChNo = S(r, "CO_CH_NO"),
                            Pcs = (int)D(r, "PCS"),
                            Qty = D(r, "QTY"),
                            Rate = D(r, "RATE"),
                            Per = S(r, "PER"),
                            Amount = D(r, "AMOUNT"),
                        });
                }

                d.AmountWords = ToWords(d.GrandTotal);
                return d;
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // ════════════════════════════════════════════════════════
        //  PUBLIC ENTRY POINTS
        // ════════════════════════════════════════════════════════

        public static void SavePdf(int invoiceId, Form parent)
        {
            var data = LoadFromDb(invoiceId);
            if (data == null) return;

            using var sfd = new SaveFileDialog
            {
                Title = "Save Invoice as PDF",
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"Invoice_{SanitiseFileName(data.InvoiceNo)}_{SanitiseFileName(data.ClientName)}.pdf"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            GeneratePdf(data, sfd.FileName);
            MessageBox.Show($"PDF saved!\n\n{sfd.FileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Open(sfd.FileName);
        }

        public static void PrintInvoice(int invoiceId, Form parent)
        {
            var data = LoadFromDb(invoiceId);
            if (data == null) return;
            string tmp = Path.Combine(Path.GetTempPath(), $"inv_{invoiceId}_{Guid.NewGuid():N}.pdf");
            GeneratePdf(data, tmp);
            ShowPreview(tmp, data, parent);
        }

        // ════════════════════════════════════════════════════════
        //  CORE PDF GENERATOR — multi-page support (#15)
        // ════════════════════════════════════════════════════════
        public static void GeneratePdf(InvoiceData d, string outputPath)
        {
            var doc = new PdfDocument();
            doc.Info.Title = $"Invoice {d.InvoiceNo}";
            doc.Info.Author = d.CompanyName;
            doc.Info.Creator = "Textile Invoice App";

            // ── Pre-calculate how many rows fit on each page ──────
            // Header section height (fixed per page):
            //   border+watermark=0, header=38, meta=34, party=70, table-hdr=17 → ~159 pt
            // Footer section height (fixed on last page only):
            //   totals ~120 + words 15 + footer 60 → ~195 pt
            // Available for rows on page 1:  PageH - MT*2 - 159 - 195 = ~335 pt  → ~20 rows
            // Available for rows on continuation pages (no party/meta re-draw):
            //   PageH - MT*2 - 17(tblhdr) - 195 = ~490 pt  → ~30 rows
            const double tRowH = 16;
            const double tHdrH = 17;
            const double headerH = 159;   // fixed header block height
            const double footerH = 200;   // totals + words + footer block
            double usableFirst = PageH - MT * 2 - headerH - footerH;
            double usableCont = PageH - MT * 2 - tHdrH - footerH;
            int rowsFirstPage = Math.Max(1, (int)(usableFirst / tRowH));
            int rowsContPage = Math.Max(1, (int)(usableCont / tRowH));

            // Partition items into pages
            var pages = new System.Collections.Generic.List<System.Collections.Generic.List<InvoiceLineItem>>();
            int idx = 0;
            // Page 1
            pages.Add(d.Items.GetRange(idx, Math.Min(rowsFirstPage, d.Items.Count - idx)));
            idx += rowsFirstPage;
            // Continuation pages
            while (idx < d.Items.Count)
            {
                pages.Add(d.Items.GetRange(idx, Math.Min(rowsContPage, d.Items.Count - idx)));
                idx += rowsContPage;
            }
            if (pages.Count == 0) pages.Add(new System.Collections.Generic.List<InvoiceLineItem>());

            bool isLastPage(int p) => p == pages.Count - 1;

            for (int pageIdx = 0; pageIdx < pages.Count; pageIdx++)
            {
                var page = doc.AddPage();
                page.Width = XUnit.FromPoint(PageW);
                page.Height = XUnit.FromPoint(PageH);
                using var g = XGraphics.FromPdfPage(page);

                // ── Fonts ────────────────────────────────────────
                var fCoName = new XFont("Arial", 16, XFontStyle.Bold);
                var fInvTtl = new XFont("Arial", 13, XFontStyle.Bold);
                var fInvNo = new XFont("Arial", 10, XFontStyle.Bold);
                var fCompInf = new XFont("Arial", 7);
                var fMetaLbl = new XFont("Arial", 7);
                var fMetaVal = new XFont("Arial", 9, XFontStyle.Bold);
                var fPtyTtl = new XFont("Arial", 7, XFontStyle.Bold);
                var fPtyNm = new XFont("Arial", 8, XFontStyle.Bold);
                var fPtyDet = new XFont("Arial", 7);
                var fTblHdr = new XFont("Arial", 7, XFontStyle.Bold);
                var fTblCell = new XFont("Arial", 7.5);
                var fTblAmt = new XFont("Arial", 7.5, XFontStyle.Bold);
                var fTotLbl = new XFont("Arial", 8, XFontStyle.Bold);
                var fTotVal = new XFont("Arial", 8, XFontStyle.Bold);
                var fGrand = new XFont("Arial", 9.5, XFontStyle.Bold);
                var fWords = new XFont("Arial", 7.5);
                var fWordsB = new XFont("Arial", 7.5, XFontStyle.Bold);
                var fFtLbl = new XFont("Arial", 7.5, XFontStyle.Bold);
                var fFtTxt = new XFont("Arial", 7.5);
                var fSigFor = new XFont("Arial", 8, XFontStyle.Bold);
                var fPageNum = new XFont("Arial", 7);

                // ── Page outer border ─────────────────────────────
                g.DrawRectangle(new XPen(CBorder, 0.6),
                    ML - 8, MT - 6, CW + 16, PageH - (MT - 6) * 2);

                // ── Watermark ─────────────────────────────────────
                {
                    const double wmFontSize = 30;
                    var wf = new XFont("Arial", wmFontSize, XFontStyle.Bold);
                    var wb = new XSolidBrush(XColor.FromArgb(9, 29, 78, 216));
                    string wmText = d.CompanyName.ToUpper();
                    string wLine1 = wmText, wLine2 = "";
                    if (wmText.Length > 22)
                    {
                        int mid = wmText.Length / 2;
                        int splitL = wmText.LastIndexOf(' ', mid);
                        int splitR = wmText.IndexOf(' ', mid);
                        int split = (splitL < 0 && splitR < 0) ? mid
                                      : (splitL < 0) ? splitR : (splitR < 0) ? splitL
                                      : (mid - splitL <= splitR - mid) ? splitL : splitR;
                        wLine1 = wmText[..split].Trim(); wLine2 = wmText[split..].Trim();
                    }
                    var wmState = g.Save();
                    g.IntersectClip(new XRect(ML - 8, MT - 6, CW + 16, PageH - (MT - 6) * 2));
                    g.TranslateTransform(PageW / 2, PageH / 2); g.RotateTransform(-30);
                    double wLineH = wmFontSize * 1.25;
                    if (wLine2 == "")
                        g.DrawString(wLine1, wf, wb, new XRect(-PageW / 2, -wLineH / 2, PageW, wLineH), XStringFormats.Center);
                    else
                    {
                        g.DrawString(wLine1, wf, wb, new XRect(-PageW / 2, -(wLineH + 2), PageW, wLineH), XStringFormats.Center);
                        g.DrawString(wLine2, wf, wb, new XRect(-PageW / 2, 2, PageW, wLineH), XStringFormats.Center);
                    }
                    g.Restore(wmState);
                }

                double y = MT;

                // ── Page number (top right corner) ────────────────
                if (pages.Count > 1)
                    g.DrawString($"Page {pageIdx + 1} of {pages.Count}", fPageNum,
                        new XSolidBrush(CTextMuted),
                        new XRect(ML, y, CW, 10), XStringFormats.TopRight);

                // ── HEADER (all pages) ────────────────────────────
                double nameColW = CW * 0.60;
                double hdrH2 = 32;
                g.DrawString(d.CompanyName, fCoName, new XSolidBrush(CBlue),
                    new XRect(ML, y, nameColW, hdrH2), XStringFormats.CenterLeft);
                g.DrawString("TAX INVOICE", fInvTtl, new XSolidBrush(CTextDark),
                    new XRect(ML, y, CW, 16), XStringFormats.TopRight);
                g.DrawString($"# {d.InvoiceNo}", fInvNo, new XSolidBrush(CBlue),
                    new XRect(ML, y + 17, CW, 14), XStringFormats.TopRight);
                y += hdrH2 + 2;
                g.DrawLine(new XPen(CBorder, 0.5), ML, y, ML + CW, y); y += 3;
                string info = $"GSTIN: {d.CompanyGstin}   |   PAN: {d.CompanyPan}" +
                              $"   |   {d.CompanyAddress}   |   Ph: {d.CompanyPhone}";
                g.DrawString(info, fCompInf, new XSolidBrush(CTextMid),
                    new XRect(ML, y, CW, 10), XStringFormats.CenterLeft);
                y += 14;

                // ── META BOXES & PARTY BOXES (first page only) ────
                if (pageIdx == 0)
                {
                    double mGap = 3, mW = (CW - mGap * 2) / 3, mH = 30;
                    DrawMetaBox(g, ML, y, mW, mH, "Invoice Date", d.InvoiceDate, fMetaLbl, fMetaVal);
                    DrawMetaBox(g, ML + mW + mGap, y, mW, mH, "Challan No", d.ChallanNo, fMetaLbl, fMetaVal);
                    DrawMetaBox(g, ML + (mW + mGap) * 2, y, mW, mH, "Challan Date", d.ChallanDate, fMetaLbl, fMetaVal);
                    y += mH + 4;

                    double pGap = 4, pH = 60;
                    double billW = CW * 0.65 - pGap / 2, trnW = CW - billW - pGap;
                    DrawPartyBox(g, ML, y, billW, pH, "BILL TO", d.ClientName, d.ClientAddr, $"GSTIN: {d.ClientGstin}", $"State: {d.ClientState}", fPtyTtl, fPtyNm, fPtyDet);
                    DrawPartyBox(g, ML + billW + pGap, y, trnW, pH, "TRANSPORT", d.Transport, "", "", "", fPtyTtl, fPtyNm, fPtyDet);
                    y += pH + 6;
                }
                else
                {
                    // Continuation page: small "continued" note
                    g.DrawString($"(Continued from page {pageIdx})", fCompInf,
                        new XSolidBrush(CTextMuted), new XRect(ML, y, CW, 10), XStringFormats.CenterLeft);
                    y += 14;
                }

                // ── TABLE HEADER ──────────────────────────────────
                double[] cw2 = { 17, 118, 44, 44, 44, 22, 42, 46, 22, 0 };
                double cwSum = 0; foreach (var w in cw2) cwSum += w;
                cw2[cw2.Length - 1] = CW - cwSum;
                XStringFormat[] align2 = { XStringFormats.Center, XStringFormats.CenterLeft, XStringFormats.Center, XStringFormats.Center, XStringFormats.Center, XStringFormats.Center, XStringFormats.CenterRight, XStringFormats.CenterRight, XStringFormats.Center, XStringFormats.CenterRight };
                string[] heads = { "Sr", "Description", "HSN", "P.CH.NO", "CO.CH.NO", "PCS", "QTY", "Rate", "Per", "Amount" };
                const double tPad = 2.5;

                g.DrawRectangle(new XSolidBrush(CBlueLt), ML, y, CW, tHdrH);
                g.DrawRectangle(new XPen(CBorder, 0.4), ML, y, CW, tHdrH);
                g.DrawLine(new XPen(CBlue, 1.2), ML, y + tHdrH, ML + CW, y + tHdrH);
                double cx = ML;
                for (int i = 0; i < heads.Length; i++)
                {
                    if (i > 0) g.DrawLine(new XPen(CBorder, 0.3), cx, y, cx, y + tHdrH);
                    g.DrawString(heads[i], fTblHdr, new XSolidBrush(CBlue),
                        new XRect(cx + tPad, y, cw2[i] - tPad * 2, tHdrH), align2[i]);
                    cx += cw2[i];
                }
                y += tHdrH;

                // ── TABLE ROWS (this page's slice) ────────────────
                var pageItems = pages[pageIdx];
                for (int ri = 0; ri < pageItems.Count; ri++)
                {
                    var it = pageItems[ri];
                    g.DrawRectangle(ri % 2 == 0 ? new XSolidBrush(XColors.White) : new XSolidBrush(CRowAlt), ML, y, CW, tRowH);
                    g.DrawRectangle(new XPen(CBorder, 0.3), ML, y, CW, tRowH);
                    string[] vals = { it.Sr.ToString(), it.Description, it.HsnCode, it.PChNo, it.CoChNo, it.Pcs.ToString(), it.Qty.ToString("N3"), it.Rate.ToString("N2"), it.Per, it.Amount.ToString("N2") };
                    cx = ML;
                    for (int ci = 0; ci < vals.Length; ci++)
                    {
                        if (ci > 0) g.DrawLine(new XPen(CBorder, 0.3), cx, y, cx, y + tRowH);
                        bool isAmt = ci == vals.Length - 1;
                        g.DrawString(vals[ci], isAmt ? fTblAmt : fTblCell,
                            new XSolidBrush(isAmt ? CGreen : CTextMid),
                            new XRect(cx + tPad, y, cw2[ci] - tPad * 2, tRowH), align2[ci]);
                        cx += cw2[ci];
                    }
                    y += tRowH;
                }
                y += 6;

                // ── TOTALS + FOOTER (last page only) ──────────────
                if (isLastPage(pageIdx))
                {
                    const double totLblW = 96, totValW = 100;
                    const double totW = totLblW + totValW;
                    const double totRowH = 14, grandRowH = 18;
                    double totX = ML + CW - totW;

                    var totRows = new[] {
                        ($"Sub Total",               $"Rs. {d.SubTotal:N2}",  false),
                        ($"CGST @ {d.CgstPct:N1}%",  $"Rs. {d.CgstAmt:N2}",  false),
                        ($"SGST @ {d.SgstPct:N1}%",  $"Rs. {d.SgstAmt:N2}",  false),
                        ($"IGST @ {d.IgstPct:N1}%",  $"Rs. {d.IgstAmt:N2}",  false),
                        ($"Roundup",                  $"Rs. {d.Roundup:N2}",   false),
                        ($"GRAND TOTAL",              $"Rs. {d.GrandTotal:N2}",true),
                    };

                    foreach (var (lbl, val, grand) in totRows)
                    {
                        double rowH = grand ? grandRowH : totRowH;
                        var bgL = grand ? new XSolidBrush(CBlueLt) : new XSolidBrush(CSlate);
                        var bgV = grand ? new XSolidBrush(CBlueLt) : new XSolidBrush(XColors.White);
                        var pen = grand ? new XPen(CBlue, 1.0) : new XPen(CBorder, 0.3);
                        var fL = grand ? fGrand : fTotLbl;
                        var fV = grand ? fGrand : fTotVal;
                        var cL = grand ? new XSolidBrush(CBlue) : new XSolidBrush(CTextMid);
                        var cV = grand ? new XSolidBrush(CGreen) : new XSolidBrush(CTextDark);
                        g.DrawRectangle(bgL, totX, y, totLblW, rowH);
                        g.DrawRectangle(bgV, totX + totLblW, y, totValW, rowH);
                        g.DrawRectangle(pen, totX, y, totW, rowH);
                        g.DrawLine(pen, totX + totLblW, y, totX + totLblW, y + rowH);
                        g.DrawString(lbl, fL, cL, new XRect(totX + 4, y, totLblW - 6, rowH), XStringFormats.CenterLeft);
                        g.DrawString(val, fV, cV, new XRect(totX + totLblW + 3, y, totValW - 5, rowH), XStringFormats.CenterRight);
                        y += rowH;
                    }
                    y += 5;

                    g.DrawString("Amount in Words:", fWordsB, new XSolidBrush(CTextMid), new XRect(ML, y, 82, 11), XStringFormats.CenterLeft);
                    g.DrawString(d.AmountWords, fWords, new XSolidBrush(CTextDark), new XRect(ML + 84, y, CW - 84 - totW - 8, 11), XStringFormats.CenterLeft);
                    y += 15;

                    double footY = Math.Max(y + 8, PageH - MT - 46);
                    g.DrawLine(new XPen(CBorder, 0.5), ML, footY, ML + CW, footY); footY += 5;
                    g.DrawString("Terms & Conditions:", fFtLbl, new XSolidBrush(CTextDark), new XRect(ML, footY, 130, 10), XStringFormats.CenterLeft); footY += 11;
                    g.DrawString("1. Goods once sold will not be taken back.", fFtTxt, new XSolidBrush(CTextMid), new XRect(ML, footY, 220, 9), XStringFormats.CenterLeft); footY += 10;
                    g.DrawString("2. Subject to local jurisdiction only.", fFtTxt, new XSolidBrush(CTextMid), new XRect(ML, footY, 220, 9), XStringFormats.CenterLeft);

                    double sigBaseY = Math.Max(y + 8, PageH - MT - 46);
                    double sigX = ML + CW - 135;
                    g.DrawString($"For {d.CompanyName}", fSigFor, new XSolidBrush(CTextDark), new XRect(sigX, sigBaseY + 3, 135, 10), XStringFormats.CenterLeft);
                    g.DrawLine(new XPen(CBorder, 0.6), sigX, sigBaseY + 30, sigX + 135, sigBaseY + 30);
                    g.DrawString("Authorised Signatory", fFtTxt, new XSolidBrush(CTextMuted), new XRect(sigX, sigBaseY + 32, 135, 9), XStringFormats.CenterLeft);
                }
            } // end page loop

            doc.Save(outputPath);
        }

        // ════════════════════════════════════════════════════════
        //  PREVIEW FORM — in-app WebBrowser (#10)
        // ════════════════════════════════════════════════════════
        public static void ShowPreview(string pdfPath, InvoiceData data, Form parent)
        {
            var frm = new Form
            {
                Text = $"Invoice #{data.InvoiceNo} — {data.ClientName}",
                Size = new Size(940, 740),
                MinimumSize = new Size(700, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(245, 247, 250),
                WindowState = FormWindowState.Normal
            };

            // ── Toolbar ───────────────────────────────────────────
            var toolbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White };
            toolbar.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(226, 232, 240) });

            var lbl = new Label
            {
                Text = $"Invoice #{data.InvoiceNo}  ·  {data.ClientName}  ·  {data.InvoiceDate}",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                AutoSize = true,
                Location = new Point(12, 16)
            };
            toolbar.Controls.Add(lbl);

            var btnPrint = MakeBtn("🖨  Print", Color.FromArgb(37, 99, 235));
            var btnSave = MakeBtn("💾  Save Copy", Color.FromArgb(22, 163, 74));
            var btnClose = MakeBtn("✕  Close", Color.FromArgb(100, 116, 139));

            void Place()
            {
                int r = frm.ClientSize.Width;
                btnClose.Location = new Point(r - 108, 10);
                btnSave.Location = new Point(r - 220, 10);
                btnPrint.Location = new Point(r - 332, 10);
            }
            Place();
            frm.Resize += (_, _) => Place();
            toolbar.Controls.Add(btnPrint);
            toolbar.Controls.Add(btnSave);
            toolbar.Controls.Add(btnClose);

            // ── In-app PDF viewer via WebBrowser ──────────────────
            // The WebBrowser control uses the installed Edge/IE runtime
            // to render the PDF inline — no external app opens.
            var browser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                ScrollBarsEnabled = true,
                IsWebBrowserContextMenuEnabled = false
            };

            // Try to navigate to the PDF file directly.
            // Modern Edge WebView2 renders PDFs natively.
            // If browser can't render PDF it falls back to showing a download link.
            browser.Navigate(new Uri(pdfPath));

            btnPrint.Click += (_, _) => {
                try { browser.ShowPrintDialog(); }
                catch { Open(pdfPath); }   // fallback: open in external viewer then print
            };

            btnSave.Click += (_, _) => {
                using var sfd = new SaveFileDialog
                {
                    Title = "Save Invoice PDF",
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Invoice_{SanitiseFileName(data.InvoiceNo)}.pdf"
                };
                if (sfd.ShowDialog() != DialogResult.OK) return;
                File.Copy(pdfPath, sfd.FileName, overwrite: true);
                MessageBox.Show($"Saved:\n{sfd.FileName}", "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            btnClose.Click += (_, _) => frm.Close();

            frm.Controls.Add(browser);
            frm.Controls.Add(toolbar);
            frm.ShowDialog(parent);
        }

        // ════════════════════════════════════════════════════════
        //  DRAWING HELPERS
        // ════════════════════════════════════════════════════════

        /// <summary>
        /// Meta box: label band (top ~11pt) + value band (below).
        /// The two bands never share the same vertical space.
        /// </summary>
        private static void DrawMetaBox(XGraphics g,
            double x, double y, double w, double h,
            string label, string value,
            XFont lblFont, XFont valFont)
        {
            g.DrawRectangle(new XSolidBrush(CSlate), x, y, w, h);
            g.DrawRectangle(new XPen(CBorder, 0.4), x, y, w, h);

            // Label — top 11 pt band
            g.DrawString(label, lblFont, new XSolidBrush(CTextMuted),
                new XRect(x + 4, y + 3, w - 8, 10), XStringFormats.TopLeft);

            // Value — starts at y + 13, never overlaps label
            g.DrawString(value, valFont, new XSolidBrush(CTextDark),
                new XRect(x + 4, y + 14, w - 8, h - 16), XStringFormats.TopLeft);
        }

        /// <summary>
        /// Party box: blue left accent + title + rule + name + up to 3 detail lines.
        /// Each element occupies its own vertical band — no overlap possible.
        /// </summary>
        private static void DrawPartyBox(XGraphics g,
            double x, double y, double w, double h,
            string title, string name,
            string line1, string line2, string line3,
            XFont titleFont, XFont nameFont, XFont detailFont)
        {
            g.DrawRectangle(new XSolidBrush(XColor.FromArgb(248, 250, 252)), x, y, w, h);
            g.DrawRectangle(new XPen(CBorder, 0.4), x, y, w, h);
            g.DrawRectangle(new XSolidBrush(CBlue), x, y, 3, h);   // blue left accent

            double ix = x + 7;
            double iw = w - 11;
            double iy = y + 4;

            // Section title (e.g. "BILL TO")
            g.DrawString(title, titleFont, new XSolidBrush(CBlue),
                new XRect(ix, iy, iw, 9), XStringFormats.TopLeft);
            iy += 10;

            // Thin separator line under title
            g.DrawLine(new XPen(XColor.FromArgb(226, 232, 240), 0.4),
                ix, iy, x + w - 5, iy);
            iy += 3;

            // Party name (bold, larger)
            g.DrawString(name, nameFont, new XSolidBrush(CTextDark),
                new XRect(ix, iy, iw, 10), XStringFormats.TopLeft);
            iy += 11;

            // Up to 3 detail lines — each 9 pt tall, clipped if box is too short
            foreach (var ln in new[] { line1, line2, line3 })
            {
                if (string.IsNullOrWhiteSpace(ln)) continue;
                if (iy + 9 > y + h - 2) break;   // graceful clip — never overflows box
                g.DrawString(ln, detailFont, new XSolidBrush(CTextMid),
                    new XRect(ix, iy, iw, 9), XStringFormats.TopLeft);
                iy += 9;
            }
        }

        private static Button MakeBtn(string text, Color bg)
        {
            var b = new Button
            {
                Text = text,
                BackColor = bg,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Size = new Size(105, 32),
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static void Open(string path) =>
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });

        // ════════════════════════════════════════════════════════
        //  AMOUNT IN WORDS  (Indian numbering)
        // ════════════════════════════════════════════════════════
        private static string ToWords(decimal amount)
        {
            long r = (long)Math.Floor(amount);
            int p = (int)Math.Round((amount - r) * 100);
            string w = WordsFor(r) + " RUPEES";
            if (p > 0) w += " AND " + WordsFor(p) + " PAISE";
            return w + " ONLY";
        }

        private static string WordsFor(long n)
        {
            if (n == 0) return "ZERO";
            string[] o = { "","ONE","TWO","THREE","FOUR","FIVE","SIX","SEVEN",
                "EIGHT","NINE","TEN","ELEVEN","TWELVE","THIRTEEN","FOURTEEN",
                "FIFTEEN","SIXTEEN","SEVENTEEN","EIGHTEEN","NINETEEN" };
            string[] t = { "","","TWENTY","THIRTY","FORTY","FIFTY",
                "SIXTY","SEVENTY","EIGHTY","NINETY" };
            string w = "";
            if (n >= 10_000_000) { w += WordsFor(n / 10_000_000) + " CRORE "; n %= 10_000_000; }
            if (n >= 100_000) { w += WordsFor(n / 100_000) + " LAKH "; n %= 100_000; }
            if (n >= 1_000) { w += WordsFor(n / 1_000) + " THOUSAND "; n %= 1_000; }
            if (n >= 100) { w += o[n / 100] + " HUNDRED "; n %= 100; }
            if (n >= 20) { w += t[n / 10] + " "; n %= 10; }
            if (n > 0) { w += o[n] + " "; }
            return w.Trim();
        }

        private static string S(System.Data.IDataRecord r, string col) =>
            r[col] == DBNull.Value ? "" : r[col]?.ToString() ?? "";
        private static decimal D(System.Data.IDataRecord r, string col) =>
            r[col] == DBNull.Value ? 0 :
            decimal.TryParse(r[col]?.ToString(), out var v) ? v : 0;
        private static string NonEmpty(string s) =>
            string.IsNullOrWhiteSpace(s) ? "—" : s;

        /// <summary>
        /// Replaces all Windows-invalid filename characters with underscores
        /// so that invoice numbers like RR/2526/000010 become RR_2526_000010.
        /// Also collapses spaces to underscores for cleaner file names.
        /// </summary>
        private static string SanitiseFileName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "_";
            // Replace every character that is invalid in a Windows filename
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            // Also collapse spaces to underscores
            s = s.Replace(' ', '_');
            // Trim trailing dots/underscores that can confuse Windows Explorer
            return s.Trim('.', '_');
        }
    }
}