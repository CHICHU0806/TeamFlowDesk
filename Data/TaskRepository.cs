using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Data;

public static class TaskRepository
{
    public static List<TaskItem> GetAll()
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
        """
        SELECT
            t.Id,
            t.ProjectId,
            COALESCE(p.Name, '未关联项目') AS ProjectName,
            t.Title,
            t.Description,
            t.OwnerName,
            t.Collaborators,
            t.Status,
            t.Priority,
            t.RiskLevel,
            t.Deadline,
            t.RelatedEquipment,
            t.OutputRequirement
        FROM Tasks AS t
        LEFT JOIN Projects AS p ON p.Id = t.ProjectId
        ORDER BY t.Id DESC;
        """;

        var tasks = new List<TaskItem>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            tasks.Add(new TaskItem
            {
                Id = reader.GetInt32(0),
                ProjectId = reader.GetInt32(1),
                ProjectName = reader.GetString(2),
                Title = reader.GetString(3),
                Description = reader.GetString(4),
                OwnerName = reader.GetString(5),
                Collaborators = reader.GetString(6),
                Status = reader.GetString(7),
                Priority = reader.GetString(8),
                RiskLevel = reader.GetString(9),
                Deadline = DateTimeOffset.Parse(reader.GetString(10)),
                RelatedEquipment = reader.GetString(11),
                OutputRequirement = reader.GetString(12)
            });
        }

        return tasks;
    }

    public static void SeedIfEmpty()
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Tasks;";

        var count = Convert.ToInt32(countCommand.ExecuteScalar());

        if (count > 0)
        {
            return;
        }

        foreach (var task in MockDataService.GetTasks())
        {
            Add(task);
        }
    }

    public static int Add(TaskItem task)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO Tasks (
            ProjectId,
            Title,
            Description,
            OwnerName,
            Collaborators,
            Status,
            Priority,
            RiskLevel,
            Deadline,
            RelatedEquipment,
            OutputRequirement
        )
        VALUES (
            $projectId,
            $title,
            $description,
            $ownerName,
            $collaborators,
            $status,
            $priority,
            $riskLevel,
            $deadline,
            $relatedEquipment,
            $outputRequirement
        );

        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue("$projectId", task.ProjectId);
        command.Parameters.AddWithValue("$title", task.Title);
        command.Parameters.AddWithValue("$description", task.Description);
        command.Parameters.AddWithValue("$ownerName", task.OwnerName);
        command.Parameters.AddWithValue("$collaborators", task.Collaborators);
        command.Parameters.AddWithValue("$status", task.Status);
        command.Parameters.AddWithValue("$priority", task.Priority);
        command.Parameters.AddWithValue("$riskLevel", task.RiskLevel);
        command.Parameters.AddWithValue("$deadline", task.Deadline.ToString("O"));
        command.Parameters.AddWithValue("$relatedEquipment", task.RelatedEquipment);
        command.Parameters.AddWithValue("$outputRequirement", task.OutputRequirement);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public static void Update(TaskItem task)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE Tasks
        SET
            ProjectId = $projectId,
            Title = $title,
            Description = $description,
            OwnerName = $ownerName,
            Collaborators = $collaborators,
            Status = $status,
            Priority = $priority,
            RiskLevel = $riskLevel,
            Deadline = $deadline,
            RelatedEquipment = $relatedEquipment,
            OutputRequirement = $outputRequirement
        WHERE Id = $id;
        """;

        command.Parameters.AddWithValue("$id", task.Id);
        command.Parameters.AddWithValue("$projectId", task.ProjectId);
        command.Parameters.AddWithValue("$title", task.Title);
        command.Parameters.AddWithValue("$description", task.Description);
        command.Parameters.AddWithValue("$ownerName", task.OwnerName);
        command.Parameters.AddWithValue("$collaborators", task.Collaborators);
        command.Parameters.AddWithValue("$status", task.Status);
        command.Parameters.AddWithValue("$priority", task.Priority);
        command.Parameters.AddWithValue("$riskLevel", task.RiskLevel);
        command.Parameters.AddWithValue("$deadline", task.Deadline.ToString("O"));
        command.Parameters.AddWithValue("$relatedEquipment", task.RelatedEquipment);
        command.Parameters.AddWithValue("$outputRequirement", task.OutputRequirement);

        command.ExecuteNonQuery();
    }

    public static void Delete(int taskId)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Tasks WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", taskId);

        command.ExecuteNonQuery();
    }
}
