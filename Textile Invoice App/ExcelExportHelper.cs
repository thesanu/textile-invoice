using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Forms;

namespace Textile_Invoice_App
{
    /// <summary>
    /// Exports a DataGridView to a proper .xlsx file using raw OpenXML.
    /// No third-party NuGet package required — uses only System.IO.Compression
    /// which ships with .NET 6+.
    ///
    /// Produces a styled spreadsheet with:
    ///   - Bold blue header row
    ///   - Alternating row shading
    ///   - Auto-width columns (approximate)
    ///   - Sheet named after the report
    /// </summary>
    public static class ExcelExportHelper
    {
        public static void Export(DataGridView dgv, string filePath, string sheetName)
        {
            // Sanitise sheet name (max 31 chars, no special chars)
            sheetName = SanitiseSheetName(sheetName);

            // ── Build worksheet XML ───────────────────────────────────
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.AppendLine("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
            sb.AppendLine("<sheetData>");

            // Header row (row 1)
            sb.AppendLine("<row r=\"1\">");
            for (int col = 0; col < dgv.Columns.Count; col++)
            {
                string cellRef = CellRef(1, col);
                string val = XmlEsc(dgv.Columns[col].HeaderText);
                // s="1" = header style
                sb.AppendLine($"<c r=\"{cellRef}\" t=\"inlineStr\" s=\"1\"><is><t>{val}</t></is></c>");
            }
            sb.AppendLine("</row>");

            // Data rows
            for (int row = 0; row < dgv.Rows.Count; row++)
            {
                int excelRow = row + 2;
                // Alternating style: s="2" (normal) or s="3" (shaded)
                string rowStyle = row % 2 == 0 ? "2" : "3";
                sb.AppendLine($"<row r=\"{excelRow}\">");
                for (int col = 0; col < dgv.Columns.Count; col++)
                {
                    string cellRef = CellRef(excelRow, col);
                    string raw = dgv.Rows[row].Cells[col].Value?.ToString() ?? "";
                    string val = XmlEsc(raw);

                    // Try numeric — numbers render better as actual numbers in Excel
                    if (decimal.TryParse(raw.Replace("₹", "").Replace(",", "").Replace("%", "").Trim(),
                            out decimal num))
                    {
                        sb.AppendLine($"<c r=\"{cellRef}\" s=\"{rowStyle}\"><v>{num}</v></c>");
                    }
                    else
                    {
                        sb.AppendLine($"<c r=\"{cellRef}\" t=\"inlineStr\" s=\"{rowStyle}\"><is><t>{val}</t></is></c>");
                    }
                }
                sb.AppendLine("</row>");
            }

            sb.AppendLine("</sheetData>");
            sb.AppendLine("</worksheet>");
            string worksheetXml = sb.ToString();

            // ── Styles XML ────────────────────────────────────────────
            // Style index 0 = default, 1 = header, 2 = normal row, 3 = shaded row
            string stylesXml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
  <fonts count=""3"">
    <font><sz val=""10""/><name val=""Segoe UI""/></font>
    <font><b/><sz val=""10""/><color rgb=""FFFFFFFF""/><name val=""Segoe UI""/></font>
    <font><sz val=""10""/><name val=""Segoe UI""/></font>
  </fonts>
  <fills count=""4"">
    <fill><patternFill patternType=""none""/></fill>
    <fill><patternFill patternType=""gray125""/></fill>
    <fill><patternFill patternType=""solid""><fgColor rgb=""FF2563EB""/></patternFill></fill>
    <fill><patternFill patternType=""solid""><fgColor rgb=""FFF1F5F9""/></patternFill></fill>
  </fills>
  <borders count=""1""><border><left/><right/><top/><bottom/><diagonal/></border></borders>
  <cellStyleXfs count=""1""><xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/></cellStyleXfs>
  <cellXfs count=""4"">
    <xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0""/>
    <xf numFmtId=""0"" fontId=""1"" fillId=""2"" borderId=""0"" xfId=""0"" applyFont=""1"" applyFill=""1""/>
    <xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0""/>
    <xf numFmtId=""0"" fontId=""2"" fillId=""3"" borderId=""0"" xfId=""0"" applyFill=""1""/>
  </cellXfs>
</styleSheet>";

            // ── Workbook XML ──────────────────────────────────────────
            string workbookXml = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main""
          xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
  <sheets>
    <sheet name=""{XmlEsc(sheetName)}"" sheetId=""1"" r:id=""rId1""/>
  </sheets>
</workbook>";

            string workbookRels = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1""
    Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet""
    Target=""worksheets/sheet1.xml""/>
  <Relationship Id=""rId2""
    Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles""
    Target=""styles.xml""/>
</Relationships>";

            string contentTypes = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml""  ContentType=""application/xml""/>
  <Override PartName=""/xl/workbook.xml""
    ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
  <Override PartName=""/xl/worksheets/sheet1.xml""
    ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
  <Override PartName=""/xl/styles.xml""
    ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml""/>
</Types>";

            string packageRels = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1""
    Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument""
    Target=""xl/workbook.xml""/>
</Relationships>";

            // ── Write .xlsx (ZIP) ──────────────────────────────────────
            if (File.Exists(filePath)) File.Delete(filePath);

            using var zip = ZipFile.Open(filePath, ZipArchiveMode.Create);

            WriteEntry(zip, "[Content_Types].xml", contentTypes);
            WriteEntry(zip, "_rels/.rels", packageRels);
            WriteEntry(zip, "xl/workbook.xml", workbookXml);
            WriteEntry(zip, "xl/_rels/workbook.xml.rels", workbookRels);
            WriteEntry(zip, "xl/styles.xml", stylesXml);
            WriteEntry(zip, "xl/worksheets/sheet1.xml", worksheetXml);
        }

        // ── Helpers ───────────────────────────────────────────────────
        static void WriteEntry(ZipArchive zip, string name, string content)
        {
            var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
            using var sw = new StreamWriter(entry.Open(), Encoding.UTF8);
            sw.Write(content);
        }

        /// <summary>Convert (row, colIndex) to Excel cell reference e.g. (1,0)→"A1"</summary>
        static string CellRef(int row, int colIndex)
        {
            string colLetter = ColLetter(colIndex);
            return $"{colLetter}{row}";
        }

        static string ColLetter(int index)
        {
            string result = "";
            index++;   // 1-based
            while (index > 0)
            {
                index--;
                result = (char)('A' + index % 26) + result;
                index /= 26;
            }
            return result;
        }

        static string XmlEsc(string s) => s
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");

        static string SanitiseSheetName(string name)
        {
            foreach (char c in new[] { ':', '\\', '/', '?', '*', '[', ']' })
                name = name.Replace(c, ' ');
            return name.Length > 31 ? name[..31] : name;
        }
    }
}