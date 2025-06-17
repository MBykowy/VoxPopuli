using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using VoxPopuli.Models.ViewModels.Surveys;

namespace VoxPopuli.Services.PDF
{
    public class SurveyResponsesDocument : IDocument
    {
        private readonly SurveyResponsesViewModel _survey;

        public SurveyResponsesDocument(SurveyResponsesViewModel survey)
        {
            _survey = survey;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
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
                        c.Item().Text($"Survey Responses: {_survey.Title}")
                            .FontSize(20).Bold();
                        c.Item().Text($"Total Responses: {_survey.ResponseCount}")
                            .FontSize(14);
                        c.Item().Height(10);
                        c.Item().Text($"Created: {_survey.CreatedAt:MMMM dd, yyyy}")
                            .FontSize(10);
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
                // Survey description
                column.Item().PaddingTop(10).Text(_survey.Description ?? "No description provided.")
                    .FontSize(12);

                // Response summary
                column.Item().PaddingVertical(10)
                    .Text("Response Summary").FontSize(16).Bold();

                ComposeResponseSummary(column.Item());

                if (_survey.Responses != null && _survey.Responses.Any())
                {
                    column.Item().PaddingTop(20)
                        .Text($"Individual Responses ({_survey.ResponseCount})").FontSize(16).Bold();

                    // Individual responses
                    foreach (var response in _survey.Responses.OrderByDescending(r => r.SubmittedAt))
                    {
                        column.Item().PaddingTop(15).Element(c => ComposeResponseDetail(c, response));
                    }
                }
                else
                {
                    column.Item().PaddingTop(10).Text("No responses recorded for this survey.").Italic();
                }
            });
        }

        private void ComposeResponseSummary(IContainer container)
        {
            container.Column(column =>
            {
                // Display metrics
                column.Item().Grid(grid =>
                {
                    grid.Columns(2);

                    grid.Item().BorderRight(1).BorderColor(Colors.Grey.Lighten2)
                        .PaddingRight(10).Column(c =>
                        {
                            c.Item().Text("Completion Rate").FontSize(12);
                            c.Item().Text($"{_survey.CompletionRate}%").FontSize(16).Bold();
                        });

                    grid.Item().PaddingLeft(10).Column(c =>
                    {
                        c.Item().Text("Avg. Completion Time").FontSize(12);
                        c.Item().Text($"{_survey.AvgCompletionTime} minutes").FontSize(16).Bold();
                    });
                });

                // Timeline data
                if (_survey.ResponseDateCounts != null && _survey.ResponseDateCounts.Any())
                {
                    column.Item().PaddingVertical(10).Text("Response Timeline").FontSize(14).Bold();
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Date").Bold();
                            header.Cell().Text("Count").Bold();
                        });

                        foreach (var dateCount in _survey.ResponseDateCounts.OrderBy(d =>
                            DateTime.Parse(d.Date, CultureInfo.InvariantCulture)))
                        {
                            table.Cell().Text(dateCount.Date);
                            table.Cell().Text(dateCount.Count.ToString());
                        }
                    });
                }
            });
        }

        private void ComposeResponseDetail(IContainer container, ResponseDetailViewModel response)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
            {
                // Response header
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        string respondent = response.IsAnonymous ? "Anonymous Response" : $"Respondent: {response.RespondentName}";
                        c.Item().Text(respondent).FontSize(14).Bold();
                    });

                    row.ConstantItem(150).AlignRight().Text(response.SubmittedAt.ToString("MMM dd, yyyy HH:mm"))
                        .FontSize(10);
                });

                column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingBottom(5);

                // Response answers
                if (response.Answers != null && response.Answers.Any())
                {
                    column.Item().PaddingTop(5);
                    foreach (var answer in response.Answers)
                    {
                        column.Item().PaddingVertical(3).Element(c => ComposeAnswer(c, answer));
                    }
                }
                else
                {
                    column.Item().PaddingTop(5).Text("No answers recorded.").Italic();
                }
            });
        }

        private void ComposeAnswer(IContainer container, ResponseAnswerViewModel answer)
        {
            container.Column(column =>
            {
                column.Item().Text(answer.QuestionText).FontSize(12).Bold();

                // Display the answer based on what data is available
                if (!string.IsNullOrEmpty(answer.SelectedOptionText))
                {
                    column.Item().Text(answer.SelectedOptionText);
                }
                else if (!string.IsNullOrEmpty(answer.AnswerText))
                {
                    column.Item().PaddingLeft(10).Text(answer.AnswerText);
                }
                else if (answer.Rating.HasValue)
                {
                    column.Item().Text($"Rating: {answer.Rating}/5");
                }
                else
                {
                    column.Item().Text("No answer provided").Italic();
                }
            });
        }
    }
}