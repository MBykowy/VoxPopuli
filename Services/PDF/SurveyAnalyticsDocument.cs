using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using VoxPopuli.Models.ViewModels.Dashboard;

namespace VoxPopuli.Services.PDF
{
    public class SurveyAnalyticsDocument : IDocument
    {
        private readonly int _totalSurveys;
        private readonly int _totalResponses;
        private readonly int _activeSurveys;
        private readonly decimal _avgResponseRate;
        private readonly List<TopSurveyViewModel> _topSurveys;
        private readonly List<RecentActivityViewModel> _recentActivities;

        public SurveyAnalyticsDocument(
            int totalSurveys,
            int totalResponses,
            int activeSurveys,
            decimal avgResponseRate,
            List<TopSurveyViewModel> topSurveys,
            List<RecentActivityViewModel> recentActivities)
        {
            _totalSurveys = totalSurveys;
            _totalResponses = totalResponses;
            _activeSurveys = activeSurveys;
            _avgResponseRate = avgResponseRate;
            _topSurveys = topSurveys;
            _recentActivities = recentActivities;
        }

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm}");
                        text.Span(" | ");
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
        }

        private void ComposeHeader(IContainer container)
        {
            // Use a column container to properly organize header elements vertically
            container.Column(column =>
            {
                // First item contains the header content in a row
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Survey Analytics Dashboard")
                            .FontSize(20).Bold();
                        c.Item().Text("Global Survey System Overview")
                            .FontSize(14);
                        c.Item().Height(10);
                    });

                    row.ConstantItem(100).Image(Placeholders.Image(100, 50));
                });

                // Second item is just for the border styling
                column.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                // Summary statistics
                column.Item().Element(ComposeStatsSummary);

                // Top performing surveys
                column.Item().PaddingTop(20).Text("Top Performing Surveys").FontSize(16).Bold();
                column.Item().Element(ComposeTopSurveys);

                // Recent activity
                column.Item().PaddingTop(20).Text("Recent Activity").FontSize(16).Bold();
                column.Item().Element(ComposeRecentActivity);
            });
        }

        private void ComposeStatsSummary(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(10).Text("Survey System Statistics").FontSize(16).Bold();

                column.Item().Grid(grid =>
                {
                    grid.Columns(4);

                    grid.Item(2).BorderRight(1).BorderColor(Colors.Grey.Lighten2)
                        .PaddingRight(10).Column(c =>
                        {
                            c.Item().Text("Total Surveys").FontSize(12);
                            c.Item().Text(_totalSurveys.ToString()).FontSize(20).Bold();
                        });

                    grid.Item(2).PaddingLeft(10).Column(c =>
                    {
                        c.Item().Text("Total Responses").FontSize(12);
                        c.Item().Text(_totalResponses.ToString()).FontSize(20).Bold();
                    });

                    grid.Item(2).BorderTop(1).BorderRight(1).BorderColor(Colors.Grey.Lighten2)
                        .PaddingTop(10).PaddingRight(10).Column(c =>
                        {
                            c.Item().Text("Active Surveys").FontSize(12);
                            c.Item().Text(_activeSurveys.ToString()).FontSize(20).Bold();
                        });

                    grid.Item(2).BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                        .PaddingTop(10).PaddingLeft(10).Column(c =>
                        {
                            c.Item().Text("Average Response Rate").FontSize(12);
                            c.Item().Text($"{_avgResponseRate}%").FontSize(20).Bold();
                        });
                });
            });
        }

        private void ComposeTopSurveys(IContainer container)
        {
            if (_topSurveys == null || !_topSurveys.Any())
            {
                container.Text("No survey data available.").Italic();
                return;
            }

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(5);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Survey Title").Bold();
                    header.Cell().Text("Responses").Bold();
                    header.Cell().Text("Completion Rate").Bold();
                });

                foreach (var survey in _topSurveys)
                {
                    table.Cell().Text(survey.Title);
                    table.Cell().Text(survey.ResponseCount.ToString());
                    table.Cell().Text($"{survey.CompletionRate}%");
                }
            });
        }

        private void ComposeRecentActivity(IContainer container)
        {
            if (_recentActivities == null || !_recentActivities.Any())
            {
                container.Text("No recent activity.").Italic();
                return;
            }

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Survey").Bold();
                    header.Cell().Text("Activity").Bold();
                    header.Cell().Text("User").Bold();
                    header.Cell().Text("Timestamp").Bold();
                });

                foreach (var activity in _recentActivities)
                {
                    table.Cell().Text(activity.SurveyTitle);
                    table.Cell().Text(activity.ActivityType);
                    table.Cell().Text(activity.Username);
                    table.Cell().Text(activity.Timestamp.ToString("g"));
                }
            });
        }
    }
}