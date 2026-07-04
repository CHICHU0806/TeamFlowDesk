using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Data;

public static class AiRecordRepository
{
    public static List<AiRecordItem> GetAll()
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        SELECT
            Id,
            RelatedModule,
            Question,
            AiSuggestion,
            HumanJudgement,
            FinalDecision,
            AdoptionStatus,
            CreatedAt
        FROM AiRecords
        ORDER BY Id DESC;
        """;

        var records = new List<AiRecordItem>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            records.Add(new AiRecordItem
            {
                Id = reader.GetInt32(0),
                RelatedModule = reader.GetString(1),
                Question = reader.GetString(2),
                AiSuggestion = reader.GetString(3),
                HumanJudgement = reader.GetString(4),
                FinalDecision = reader.GetString(5),
                AdoptionStatus = reader.GetString(6),
                CreatedAt = DateTimeOffset.Parse(reader.GetString(7))
            });
        }

        return records;
    }

    public static void SeedIfEmpty()
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM AiRecords;";

        var count = Convert.ToInt32(countCommand.ExecuteScalar());

        if (count > 0)
        {
            return;
        }

        foreach (var record in MockDataService.GetAiRecords())
        {
            Add(record);
        }
    }

    public static int Add(AiRecordItem record)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO AiRecords (
            RelatedModule,
            Question,
            AiSuggestion,
            HumanJudgement,
            FinalDecision,
            AdoptionStatus,
            CreatedAt
        )
        VALUES (
            $relatedModule,
            $question,
            $aiSuggestion,
            $humanJudgement,
            $finalDecision,
            $adoptionStatus,
            $createdAt
        );

        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue("$relatedModule", record.RelatedModule);
        command.Parameters.AddWithValue("$question", record.Question);
        command.Parameters.AddWithValue("$aiSuggestion", record.AiSuggestion);
        command.Parameters.AddWithValue("$humanJudgement", record.HumanJudgement);
        command.Parameters.AddWithValue("$finalDecision", record.FinalDecision);
        command.Parameters.AddWithValue("$adoptionStatus", record.AdoptionStatus);
        command.Parameters.AddWithValue("$createdAt", record.CreatedAt.ToString("O"));

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public static void Update(AiRecordItem record)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE AiRecords
        SET
            RelatedModule = $relatedModule,
            Question = $question,
            AiSuggestion = $aiSuggestion,
            HumanJudgement = $humanJudgement,
            FinalDecision = $finalDecision,
            AdoptionStatus = $adoptionStatus,
            CreatedAt = $createdAt
        WHERE Id = $id;
        """;

        command.Parameters.AddWithValue("$id", record.Id);
        command.Parameters.AddWithValue("$relatedModule", record.RelatedModule);
        command.Parameters.AddWithValue("$question", record.Question);
        command.Parameters.AddWithValue("$aiSuggestion", record.AiSuggestion);
        command.Parameters.AddWithValue("$humanJudgement", record.HumanJudgement);
        command.Parameters.AddWithValue("$finalDecision", record.FinalDecision);
        command.Parameters.AddWithValue("$adoptionStatus", record.AdoptionStatus);
        command.Parameters.AddWithValue("$createdAt", record.CreatedAt.ToString("O"));

        command.ExecuteNonQuery();
    }

    public static void Delete(int recordId)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM AiRecords WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", recordId);

        command.ExecuteNonQuery();
    }
}