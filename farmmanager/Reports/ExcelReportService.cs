using ClosedXML.Excel;
using farmmanager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using farmmanager.Services;

namespace farmmanager.Reports
{
    public class ExcelReportService
    {
        private static readonly XLColor HeaderBg = XLColor.FromHtml("#1B5E20");
        private static readonly XLColor SubHeaderBg = XLColor.FromHtml("#2E7D32");
        private static readonly XLColor AltRow = XLColor.FromHtml("#F1F8E9");
        private static readonly XLColor TotalBg = XLColor.FromHtml("#E8F5E9");

        // ── WORKER SUMMARY REPORT ──────────────────────────────────────────────

        public async Task<string> GenerateWorkerSummaryReportAsync(
            List<WorkEntry> entries,
            List<WorkerSummary> summaries,
            DateTime fromDate, DateTime toDate,
            string savePath)
        {
            using var wb = new XLWorkbook();

            // Sheet 1: Summary
            var wsSummary = wb.AddWorksheet("Worker Summary");
            BuildWorkerSummarySheet(wsSummary, summaries, fromDate, toDate);

            // Sheet 2: Detail
            var wsDetail = wb.AddWorksheet("Daily Detail");
            BuildDetailSheet(wsDetail, entries, fromDate, toDate);

            wb.SaveAs(savePath);
            return savePath;
        }

        private void BuildWorkerSummarySheet(IXLWorksheet ws, List<WorkerSummary> summaries,
            DateTime from, DateTime to)
        {
            // Title
            ws.Cell(1, 1).Value = "PLANTATION MANAGER — WORKER SUMMARY REPORT";
            ws.Range(1, 1, 1, 7).Merge().Style
                .Font.SetBold(true).Font.SetFontSize(14).Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(HeaderBg)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(2, 1).Value = $"Period: {from:dd MMM yyyy} – {to:dd MMM yyyy}";
            ws.Range(2, 1, 2, 7).Merge().Style
                .Font.SetItalic(true).Font.SetFontColor(XLColor.DarkGray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(3, 1).Value = $"Generated: {DateTime.Now:dd MMM yyyy HH:mm}";
            ws.Range(3, 1, 3, 7).Merge().Style
                .Font.SetFontSize(9).Font.SetFontColor(XLColor.Gray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // Headers
            string[] headers = { "Worker Name", "Group", "Days Worked", "Obj. Planned", "Obj. Attained", "Completion %", "Total Earned (FCFA)" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(5, i + 1).Value = headers[i];
                ws.Cell(5, i + 1).Style
                    .Font.SetBold(true).Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(SubHeaderBg)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Border.SetBottomBorder(XLBorderStyleValues.Medium);
            }

            // Data rows
            string? currentGroup = null;
            int row = 6;
            decimal groupTotal = 0;
            int groupStart = 6;

            foreach (var s in summaries)
            {
                if (currentGroup != null && s.Group != currentGroup)
                {
                    // Group total row
                    WriteGroupTotalRow(ws, row, currentGroup!, groupTotal);
                    row++;
                    groupTotal = 0;
                    groupStart = row;
                }
                currentGroup = s.Group;

                bool isAlt = (row % 2 == 0);
                var rowStyle = ws.Row(row).Style;
                if (isAlt) rowStyle.Fill.SetBackgroundColor(AltRow);

                decimal completion = s.TotalObjectivePlanned > 0
                    ? s.TotalObjectiveAttained / s.TotalObjectivePlanned * 100 : 0;

                ws.Cell(row, 1).Value = s.WorkerName;
                ws.Cell(row, 2).Value = s.Group;
                ws.Cell(row, 3).Value = s.TotalDays;
                ws.Cell(row, 4).Value = (double)s.TotalObjectivePlanned;
                ws.Cell(row, 5).Value = (double)s.TotalObjectiveAttained;
                ws.Cell(row, 6).Value = (double)Math.Round(completion, 1);
                ws.Cell(row, 7).Value = (double)s.TotalAmountEarned;

                ws.Cell(row, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                ws.Cell(row, 6).Style.NumberFormat.SetFormat("0.0\"%\"");
                ws.Cell(row, 7).Style.NumberFormat.SetFormat("#,##0");

                groupTotal += s.TotalAmountEarned;
                row++;
            }

            // Last group total
            if (currentGroup != null)
            {
                WriteGroupTotalRow(ws, row, currentGroup, groupTotal);
                row++;
            }

            // Grand Total
            ws.Cell(row + 1, 1).Value = "GRAND TOTAL";
            ws.Cell(row + 1, 6).Value = "Total Amount:";
            ws.Cell(row + 1, 7).Value = (double)summaries.Sum(s => s.TotalAmountEarned);
            ws.Range(row + 1, 1, row + 1, 7).Style
                .Font.SetBold(true).Font.SetFontSize(12)
                .Fill.SetBackgroundColor(HeaderBg).Font.SetFontColor(XLColor.White)
                .Border.SetTopBorder(XLBorderStyleValues.Double);
            ws.Cell(row + 1, 7).Style.NumberFormat.SetFormat("#,##0");

            // Column widths
            ws.Column(1).Width = 25;
            ws.Column(2).Width = 18;
            ws.Column(3).Width = 14;
            ws.Column(4).Width = 16;
            ws.Column(5).Width = 16;
            ws.Column(6).Width = 14;
            ws.Column(7).Width = 20;

            ws.SheetView.FreezeRows(5);
        }

        private void WriteGroupTotalRow(IXLWorksheet ws, int row, string group, decimal total)
        {
            ws.Cell(row, 1).Value = $"Subtotal — {group}";
            ws.Cell(row, 7).Value = (double)total;
            ws.Range(row, 1, row, 7).Style
                .Font.SetBold(true).Fill.SetBackgroundColor(TotalBg)
                .Border.SetTopBorder(XLBorderStyleValues.Thin)
                .Border.SetBottomBorder(XLBorderStyleValues.Thin);
            ws.Cell(row, 7).Style.NumberFormat.SetFormat("#,##0");
        }

        // ── PARCEL SUMMARY REPORT ─────────────────────────────────────────────

        public string GenerateParcelSummaryReport(
            List<ParcelSummary> summaries, DateTime fromDate, DateTime toDate, string savePath)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Parcel Summary");

            SetReportTitle(ws, "PLANTATION MANAGER — PARCEL EXPENDITURE REPORT",
                fromDate, toDate, 6);

            string[] headers = { "Parcel Name", "Code", "Work Entries", "Total Area Worked", "Total Amount Spent (FCFA)", "Avg. Cost/Entry (FCFA)" };
            WriteHeaders(ws, 5, headers);

            int row = 6;
            foreach (var s in summaries)
            {
                bool isAlt = (row % 2 == 0);
                if (isAlt) ws.Row(row).Style.Fill.SetBackgroundColor(AltRow);

                ws.Cell(row, 1).Value = s.ParcelName;
                ws.Cell(row, 2).Value = s.ParcelCode;
                ws.Cell(row, 3).Value = s.TotalEntries;
                ws.Cell(row, 4).Value = (double)s.TotalAreaWorked;
                ws.Cell(row, 5).Value = (double)s.TotalAmountSpent;
                ws.Cell(row, 6).Value = s.TotalEntries > 0
                    ? (double)(s.TotalAmountSpent / s.TotalEntries) : 0;

                ws.Cell(row, 5).Style.NumberFormat.SetFormat("#,##0");
                ws.Cell(row, 6).Style.NumberFormat.SetFormat("#,##0");
                row++;
            }

            // Total row
            WriteTotalRow(ws, row, "TOTAL", 5, summaries.Sum(s => s.TotalAmountSpent), 6);

            SetColumnWidths(ws, new[] { 30.0, 12, 14, 18, 24, 22 });
            ws.SheetView.FreezeRows(5);

            wb.SaveAs(savePath);
            return savePath;
        }

        // ── ACTIVITY SUMMARY REPORT ────────────────────────────────────────────

        public string GenerateActivitySummaryReport(
            List<ActivitySummary> summaries, DateTime fromDate, DateTime toDate, string savePath)
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Activity Summary");

            SetReportTitle(ws, "PLANTATION MANAGER — ACTIVITY EXPENDITURE REPORT",
                fromDate, toDate, 7);

            string[] headers = { "Activity", "Unit", "Entries", "Obj. Planned", "Obj. Attained", "Completion %", "Total Spent (FCFA)" };
            WriteHeaders(ws, 5, headers);

            int row = 6;
            foreach (var s in summaries)
            {
                bool isAlt = (row % 2 == 0);
                if (isAlt) ws.Row(row).Style.Fill.SetBackgroundColor(AltRow);

                decimal completion = s.TotalObjectivePlanned > 0
                    ? s.TotalObjectiveAttained / s.TotalObjectivePlanned * 100 : 0;

                ws.Cell(row, 1).Value = s.ActivityName;
                ws.Cell(row, 2).Value = s.Unit;
                ws.Cell(row, 3).Value = s.TotalEntries;
                ws.Cell(row, 4).Value = (double)s.TotalObjectivePlanned;
                ws.Cell(row, 5).Value = (double)s.TotalObjectiveAttained;
                ws.Cell(row, 6).Value = (double)Math.Round(completion, 1);
                ws.Cell(row, 7).Value = (double)s.TotalAmountSpent;

                ws.Cell(row, 6).Style.NumberFormat.SetFormat("0.0\"%\"");
                ws.Cell(row, 7).Style.NumberFormat.SetFormat("#,##0");
                row++;
            }

            WriteTotalRow(ws, row, "TOTAL", 6, summaries.Sum(s => s.TotalAmountSpent), 7);

            SetColumnWidths(ws, new[] { 25.0, 12, 10, 16, 16, 14, 22 });
            ws.SheetView.FreezeRows(5);

            wb.SaveAs(savePath);
            return savePath;
        }

        // ── COMPREHENSIVE REPORT (all three sheets) ───────────────────────────

        public async Task<string> GenerateFullReportAsync(
            List<WorkerSummary> workerSummaries,
            List<ParcelSummary> parcelSummaries,
            List<ActivitySummary> activitySummaries,
            List<WorkEntry> entries,
            DateTime fromDate, DateTime toDate,
            string savePath)
        {
            using var wb = new XLWorkbook();

            var wsWorkers = wb.AddWorksheet("Workers");
            BuildWorkerSummarySheet(wsWorkers, workerSummaries, fromDate, toDate);

            var wsParcels = wb.AddWorksheet("Parcels");
            BuildParcelSheet(wsParcels, parcelSummaries, fromDate, toDate);

            var wsActivities = wb.AddWorksheet("Activities");
            BuildActivitySheet(wsActivities, activitySummaries, fromDate, toDate);

            var wsDetail = wb.AddWorksheet("Daily Log");
            BuildDetailSheet(wsDetail, entries, fromDate, toDate);

            wb.SaveAs(savePath);
            return savePath;
        }

        private void BuildDetailSheet(IXLWorksheet ws, List<WorkEntry> entries,
            DateTime from, DateTime to)
        {
            SetReportTitle(ws, "DAILY WORK LOG", from, to, 9);

            string[] headers = { "Date", "Worker", "Group", "Parcel", "Activity", "Objective", "Planned", "Attained", "Amount (FCFA)" };
            WriteHeaders(ws, 5, headers);

            int row = 6;
            foreach (var e in entries.OrderBy(x => x.Date).ThenBy(x => x.Worker.Name))
            {
                bool isAlt = (row % 2 == 0);
                if (isAlt) ws.Row(row).Style.Fill.SetBackgroundColor(AltRow);

                ws.Cell(row, 1).Value = e.Date.ToString("dd/MM/yyyy");
                ws.Cell(row, 2).Value = e.Worker.Name;
                ws.Cell(row, 3).Value = e.Worker.Group;
                ws.Cell(row, 4).Value = e.Parcel.Name;
                ws.Cell(row, 5).Value = e.Activity.Name;
                ws.Cell(row, 6).Value = e.ObjectiveOfDay;
                ws.Cell(row, 7).Value = (double)e.ObjectivePlanned;
                ws.Cell(row, 8).Value = (double)e.ObjectiveAttained;
                ws.Cell(row, 9).Value = (double)e.AmountEarned;
                ws.Cell(row, 9).Style.NumberFormat.SetFormat("#,##0");
                row++;
            }

            WriteTotalRow(ws, row, "TOTAL", 8, entries.Sum(e => e.AmountEarned), 9);
            SetColumnWidths(ws, new[] { 12.0, 22, 16, 20, 18, 30, 12, 12, 18 });
            ws.SheetView.FreezeRows(5);
        }

        private void BuildParcelSheet(IXLWorksheet ws, List<ParcelSummary> summaries, DateTime from, DateTime to)
        {
            SetReportTitle(ws, "PARCEL EXPENDITURE", from, to, 6);
            WriteHeaders(ws, 5, new[] { "Parcel Name", "Code", "Work Entries", "Total Area Worked", "Total Spent (FCFA)", "Avg/Entry (FCFA)" });
            int row = 6;
            foreach (var s in summaries)
            {
                if (row % 2 == 0) ws.Row(row).Style.Fill.SetBackgroundColor(AltRow);
                ws.Cell(row, 1).Value = s.ParcelName;
                ws.Cell(row, 2).Value = s.ParcelCode;
                ws.Cell(row, 3).Value = s.TotalEntries;
                ws.Cell(row, 4).Value = (double)s.TotalAreaWorked;
                ws.Cell(row, 5).Value = (double)s.TotalAmountSpent;
                ws.Cell(row, 6).Value = s.TotalEntries > 0 ? (double)(s.TotalAmountSpent / s.TotalEntries) : 0;
                ws.Cell(row, 5).Style.NumberFormat.SetFormat("#,##0");
                ws.Cell(row, 6).Style.NumberFormat.SetFormat("#,##0");
                row++;
            }
            WriteTotalRow(ws, row, "TOTAL", 5, summaries.Sum(s => s.TotalAmountSpent), 6);
            SetColumnWidths(ws, new[] { 28.0, 12, 14, 18, 22, 20 });
        }

        private void BuildActivitySheet(IXLWorksheet ws, List<ActivitySummary> summaries, DateTime from, DateTime to)
        {
            SetReportTitle(ws, "ACTIVITY EXPENDITURE", from, to, 7);
            WriteHeaders(ws, 5, new[] { "Activity", "Unit", "Entries", "Planned", "Attained", "Completion %", "Total Spent (FCFA)" });
            int row = 6;
            foreach (var s in summaries)
            {
                if (row % 2 == 0) ws.Row(row).Style.Fill.SetBackgroundColor(AltRow);
                decimal completion = s.TotalObjectivePlanned > 0 ? s.TotalObjectiveAttained / s.TotalObjectivePlanned * 100 : 0;
                ws.Cell(row, 1).Value = s.ActivityName;
                ws.Cell(row, 2).Value = s.Unit;
                ws.Cell(row, 3).Value = s.TotalEntries;
                ws.Cell(row, 4).Value = (double)s.TotalObjectivePlanned;
                ws.Cell(row, 5).Value = (double)s.TotalObjectiveAttained;
                ws.Cell(row, 6).Value = (double)Math.Round(completion, 1);
                ws.Cell(row, 7).Value = (double)s.TotalAmountSpent;
                ws.Cell(row, 6).Style.NumberFormat.SetFormat("0.0\"%\"");
                ws.Cell(row, 7).Style.NumberFormat.SetFormat("#,##0");
                row++;
            }
            WriteTotalRow(ws, row, "TOTAL", 6, summaries.Sum(s => s.TotalAmountSpent), 7);
            SetColumnWidths(ws, new[] { 25.0, 12, 10, 14, 14, 14, 22 });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetReportTitle(IXLWorksheet ws, string title, DateTime from, DateTime to, int cols)
        {
            ws.Cell(1, 1).Value = title;
            ws.Range(1, 1, 1, cols).Merge().Style
                .Font.SetBold(true).Font.SetFontSize(13).Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(HeaderBg)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws.Cell(2, 1).Value = $"Period: {from:dd MMM yyyy} – {to:dd MMM yyyy}  |  Generated: {DateTime.Now:dd MMM yyyy HH:mm}";
            ws.Range(2, 1, 2, cols).Merge().Style
                .Font.SetItalic(true).Font.SetFontColor(XLColor.DarkGray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        }

        private void WriteHeaders(IXLWorksheet ws, int row, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                ws.Cell(row, i + 1).Style
                    .Font.SetBold(true).Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(SubHeaderBg)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Border.SetBottomBorder(XLBorderStyleValues.Medium);
            }
        }

        private void WriteTotalRow(IXLWorksheet ws, int row, string label, int labelCol, decimal total, int totalCol)
        {
            ws.Cell(row + 1, 1).Value = label;
            ws.Cell(row + 1, totalCol).Value = (double)total;
            ws.Range(row + 1, 1, row + 1, totalCol).Style
                .Font.SetBold(true).Font.SetFontSize(11).Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(HeaderBg)
                .Border.SetTopBorder(XLBorderStyleValues.Double);
            ws.Cell(row + 1, totalCol).Style.NumberFormat.SetFormat("#,##0");
        }

        private void SetColumnWidths(IXLWorksheet ws, double[] widths)
        {
            for (int i = 0; i < widths.Length; i++)
                ws.Column(i + 1).Width = widths[i];
        }
    }

}
