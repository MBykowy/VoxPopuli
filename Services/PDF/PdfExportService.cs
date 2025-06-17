using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VoxPopuli.Models.ViewModels.Dashboard;
using VoxPopuli.Models.ViewModels.Surveys;

namespace VoxPopuli.Services.PDF
{
    public class PdfExportService
    {
        public byte[] GenerateSurveyResultPdf(SurveyResultViewModel survey)
        {
            if (survey == null)
                throw new ArgumentNullException(nameof(survey));

            try
            {
                var document = new SurveyResultDocument(survey);
                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating survey PDF: {ex.Message}");
                throw new Exception("Failed to generate survey PDF report", ex);
            }
        }




        public byte[] GenerateAnalyticsDashboardPdf(
            int totalSurveys,
            int totalResponses,
            int activeSurveys,
            decimal avgResponseRate,
            List<TopSurveyViewModel> topSurveys,
            List<RecentActivityViewModel> recentActivities)
        {
            try
            {
                var document = new SurveyAnalyticsDocument(
                    totalSurveys,
                    totalResponses,
                    activeSurveys,
                    avgResponseRate,
                    topSurveys,
                    recentActivities);

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating dashboard PDF: {ex.Message}");
                throw new Exception("Failed to generate analytics dashboard PDF report", ex);
            }
        }
        public byte[] GenerateSurveyResponsesPdf(SurveyResponsesViewModel responses)
        {
            if (responses == null)
                throw new ArgumentNullException(nameof(responses));

            try
            {
                var document = new SurveyResponsesDocument(responses);
                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating responses PDF: {ex.Message}");
                throw new Exception("Failed to generate survey responses PDF", ex);
            }
        }
        public async Task SavePdfToFileAsync(byte[] pdfData, string filePath)
        {
            await File.WriteAllBytesAsync(filePath, pdfData);
        }
    }
}