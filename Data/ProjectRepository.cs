using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Data;

public static class ProjectRepository
{
    public static List<ProjectItem> GetAll()
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            Id,
            Name,
            Description,
            OwnerName,
            Status,
            CurrentStage,
            RiskLevel,
            StartDate,
            EndDate,
            ProgressPercent
        FROM Projects
        ORDER BY Id DESC;
        """;

        var projects = new List<ProjectItem>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            projects.Add(new ProjectItem
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                OwnerName = reader.GetString(3),
                Status = reader.GetString(4),
                CurrentStage = reader.GetString(5),
                RiskLevel = reader.GetString(6),
                StartDate = DateTimeOffset.Parse(reader.GetString(7)),
                EndDate = DateTimeOffset.Parse(reader.GetString(8)),
                ProgressPercent = reader.GetInt32(9)
            });
        }

        return projects;
    }

    public static void SeedIfEmpty()
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Projects;";

        var count = Convert.ToInt32(countCommand.ExecuteScalar());

        if (count > 0)
        {
            return;
        }

        foreach (var project in MockDataService.GetProjects())
        {
            Add(project);
        }
    }

    public static int Add(ProjectItem project)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO Projects (
            Name,
            Description,
            OwnerName,
            Status,
            CurrentStage,
            RiskLevel,
            StartDate,
            EndDate,
            ProgressPercent
        )
        VALUES (
            $name,
            $description,
            $ownerName,
            $status,
            $currentStage,
            $riskLevel,
            $startDate,
            $endDate,
            $progressPercent
        );

        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue("$name", project.Name);
        command.Parameters.AddWithValue("$description", project.Description);
        command.Parameters.AddWithValue("$ownerName", project.OwnerName);
        command.Parameters.AddWithValue("$status", project.Status);
        command.Parameters.AddWithValue("$currentStage", project.CurrentStage);
        command.Parameters.AddWithValue("$riskLevel", project.RiskLevel);
        command.Parameters.AddWithValue("$startDate", project.StartDate.ToString("O"));
        command.Parameters.AddWithValue("$endDate", project.EndDate.ToString("O"));
        command.Parameters.AddWithValue("$progressPercent", project.ProgressPercent);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public static void Update(ProjectItem project)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE Projects
        SET
            Name = $name,
            Description = $description,
            OwnerName = $ownerName,
            Status = $status,
            CurrentStage = $currentStage,
            RiskLevel = $riskLevel,
            StartDate = $startDate,
            EndDate = $endDate,
            ProgressPercent = $progressPercent
        WHERE Id = $id;
        """;

        command.Parameters.AddWithValue("$id", project.Id);
        command.Parameters.AddWithValue("$name", project.Name);
        command.Parameters.AddWithValue("$description", project.Description);
        command.Parameters.AddWithValue("$ownerName", project.OwnerName);
        command.Parameters.AddWithValue("$status", project.Status);
        command.Parameters.AddWithValue("$currentStage", project.CurrentStage);
        command.Parameters.AddWithValue("$riskLevel", project.RiskLevel);
        command.Parameters.AddWithValue("$startDate", project.StartDate.ToString("O"));
        command.Parameters.AddWithValue("$endDate", project.EndDate.ToString("O"));
        command.Parameters.AddWithValue("$progressPercent", project.ProgressPercent);

        command.ExecuteNonQuery();
    }

    public static void Delete(int projectId)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Projects WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", projectId);

        command.ExecuteNonQuery();
    }
}