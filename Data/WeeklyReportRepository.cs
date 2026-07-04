using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Data;

public static class WeeklyReportRepository
{
    public static List<WeeklyReportItem> GetAll()
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            Id,
            Title,
            StartDate,
            EndDate,
            CompletedWork,
            Problems,
            NextPlan,
            AiCollaborationSummary,
            ManagerReview,
            ProgressStatus
        FROM WeeklyReports
        ORDER BY Id DESC;
        """;

        var reports = new List<WeeklyReportItem>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            reports.Add(new WeeklyReportItem
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                StartDate = DateTimeOffset.Parse(reader.GetString(2)),
                EndDate = DateTimeOffset.Parse(reader.GetString(3)),
                CompletedWork = reader.GetString(4),
                Problems = reader.GetString(5),
                NextPlan = reader.GetString(6),
                AiCollaborationSummary = reader.GetString(7),
                ManagerReview = reader.GetString(8),
                ProgressStatus = reader.GetString(9)
            });
        }

        return reports;
    }

    public static void SeedIfEmpty()
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM WeeklyReports;";

        var count = Convert.ToInt32(countCommand.ExecuteScalar());

        if (count > 0)
        {
            return;
        }

        foreach (var report in MockDataService.GetWeeklyReports())
        {
            Add(report);
        }
    }

    public static int Add(WeeklyReportItem report)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO WeeklyReports (
            Title,
            StartDate,
            EndDate,
            CompletedWork,
            Problems,
            NextPlan,
            AiCollaborationSummary,
            ManagerReview,
            ProgressStatus
        )
        VALUES (
            $title,
            $startDate,
            $endDate,
            $completedWork,
            $problems,
            $nextPlan,
            $aiCollaborationSummary,
            $managerReview,
            $progressStatus
        );

        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue("$title", report.Title);
        command.Parameters.AddWithValue("$startDate", report.StartDate.ToString("O"));
        command.Parameters.AddWithValue("$endDate", report.EndDate.ToString("O"));
        command.Parameters.AddWithValue("$completedWork", report.CompletedWork);
        command.Parameters.AddWithValue("$problems", report.Problems);
        command.Parameters.AddWithValue("$nextPlan", report.NextPlan);
        command.Parameters.AddWithValue("$aiCollaborationSummary", report.AiCollaborationSummary);
        command.Parameters.AddWithValue("$managerReview", report.ManagerReview);
        command.Parameters.AddWithValue("$progressStatus", report.ProgressStatus);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public static void Update(WeeklyReportItem report)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE WeeklyReports
        SET
            Title = $title,
            StartDate = $startDate,
            EndDate = $endDate,
            CompletedWork = $completedWork,
            Problems = $problems,
            NextPlan = $nextPlan,
            AiCollaborationSummary = $aiCollaborationSummary,
            ManagerReview = $managerReview,
            ProgressStatus = $progressStatus
        WHERE Id = $id;
        """;

        command.Parameters.AddWithValue("$id", report.Id);
        command.Parameters.AddWithValue("$title", report.Title);
        command.Parameters.AddWithValue("$startDate", report.StartDate.ToString("O"));
        command.Parameters.AddWithValue("$endDate", report.EndDate.ToString("O"));
        command.Parameters.AddWithValue("$completedWork", report.CompletedWork);
        command.Parameters.AddWithValue("$problems", report.Problems);
        command.Parameters.AddWithValue("$nextPlan", report.NextPlan);
        command.Parameters.AddWithValue("$aiCollaborationSummary", report.AiCollaborationSummary);
        command.Parameters.AddWithValue("$managerReview", report.ManagerReview);
        command.Parameters.AddWithValue("$progressStatus", report.ProgressStatus);

        command.ExecuteNonQuery();
    }

    public static void Delete(int reportId)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM WeeklyReports WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", reportId);

        command.ExecuteNonQuery();
    }
}