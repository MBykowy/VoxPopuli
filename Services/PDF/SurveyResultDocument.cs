using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using VoxPopuli.Models.ViewModels.Surveys;

namespace VoxPopuli.Services.PDF
{
    public class SurveyResultDocument : IDocument
    {
        private readonly SurveyResultViewModel _survey;

        public SurveyResultDocument(SurveyResultViewModel survey)
        {
            _survey = survey;
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
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text($"Survey Results: {_survey.Title}")
                            .FontSize(20).Bold();
                        c.Item().Text($"Total Responses: {_survey.ResponseCount}")
                            .FontSize(14);
                        c.Item().Height(10);
                        c.Item().Text($"Created: {_survey.CreatedAt:MMMM dd, yyyy}")
                            .FontSize(10);
                    });

                    row.ConstantItem(100).Image(Placeholders.Image(100, 50));
                });

                column.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(10).Text(_survey.Description ?? "No description provided.")
                    .FontSize(12);

                column.Item().PaddingVertical(10)
                    .Text("Questions and Responses").FontSize(16).Bold();

                if (_survey.Questions != null && _survey.Questions.Any())
                {
                    foreach (var question in _survey.Questions.OrderBy(q => q.Order))
                    {
                        column.Item().PaddingTop(15).Element(c => ComposeQuestion(c, question));
                    }
                }
                else
                {
                    column.Item().PaddingTop(10).Text("No responses recorded for this survey.").Italic();
                }
            });
        }

        private void ComposeQuestion(IContainer container, QuestionResultViewModel question)
        {
            container.Column(column =>
            {
                column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5);
                column.Item().Text($"Q: {question.QuestionText}").FontSize(14).Bold();

                switch (question.QuestionType)
                {
                    case Models.Domain.QuestionType.SingleChoice:
                    case Models.Domain.QuestionType.MultipleChoice:
                        ComposeChoiceQuestion(column, question);
                        break;

                    case Models.Domain.QuestionType.Rating:
                        ComposeRatingQuestion(column, question);
                        break;

                    case Models.Domain.QuestionType.Text:
                        ComposeTextQuestion(column, question);
                        break;
                }
            });
        }

        private void ComposeChoiceQuestion(ColumnDescriptor column, QuestionResultViewModel question)
        {
            if (question.Options != null && question.Options.Any())
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Option").Bold();
                        header.Cell().Text("Count").Bold();
                        header.Cell().Text("Percentage").Bold();
                    });

                    foreach (var option in question.Options.OrderByDescending(o => o.Count))
                    {
                        table.Cell().Text(option.OptionText);
                        table.Cell().Text(option.Count.ToString());
                        table.Cell().Text($"{option.Percentage}%");
                    }
                });
            }
            else
            {
                column.Item().Text("No responses recorded.").Italic();
            }
        }

        private void ComposeRatingQuestion(ColumnDescriptor column, QuestionResultViewModel question)
        {
            if (question.AverageRating.HasValue)
            {
                column.Item().Text($"Average Rating: {question.AverageRating.Value:F1}/5").Bold();

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Rating").Bold();
                        header.Cell().Text("Count").Bold();
                    });

                    for (int i = 5; i >= 1; i--)
                    {
                        table.Cell().Text($"{i} {(i == 5 ? "(Excellent)" : i == 1 ? "(Poor)" : "")}");
                        table.Cell().Text($"{(question.RatingDistribution.ContainsKey(i) ? question.RatingDistribution[i] : 0)}");
                    }
                });
            }
            else
            {
                column.Item().Text("No ratings recorded.").Italic();
            }
        }

        private void ComposeTextQuestion(ColumnDescriptor column, QuestionResultViewModel question)
        {
            if (question.TextResponses != null && question.TextResponses.Any())
            {
                column.Item().Text($"Responses ({question.TextResponses.Count})").Bold();

                foreach (var response in question.TextResponses)
                {
                    column.Item().PaddingVertical(5).Background(Colors.Grey.Lighten3).Padding(5)
                        .Text(response);
                }
            }
            else
            {
                column.Item().Text("No text responses submitted.").Italic();
            }
        }
    }
}