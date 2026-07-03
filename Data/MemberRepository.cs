using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Data;

public static class MemberRepository
{
    public static List<MemberItem> GetAll()
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
            Grade,
            Direction,
            Role,
            SkillTags,
            AbilityLevel,
            CurrentTaskCount,
            WorkloadStatus,
            GrowthPlan
        FROM Members
        ORDER BY Id DESC;
        """;

        var members = new List<MemberItem>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            members.Add(new MemberItem
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Grade = reader.GetString(2),
                Direction = reader.GetString(3),
                Role = reader.GetString(4),
                SkillTags = reader.GetString(5),
                AbilityLevel = reader.GetString(6),
                CurrentTaskCount = reader.GetInt32(7),
                WorkloadStatus = reader.GetString(8),
                GrowthPlan = reader.GetString(9)
            });
        }

        return members;
    }

    public static void SeedIfEmpty()
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Members;";

        var count = Convert.ToInt32(countCommand.ExecuteScalar());

        if (count > 0)
        {
            return;
        }

        foreach (var member in MockDataService.GetMembers())
        {
            Add(member);
        }
    }

    public static int Add(MemberItem member)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO Members (
            Name,
            Grade,
            Direction,
            Role,
            SkillTags,
            AbilityLevel,
            CurrentTaskCount,
            WorkloadStatus,
            GrowthPlan
        )
        VALUES (
            $name,
            $grade,
            $direction,
            $role,
            $skillTags,
            $abilityLevel,
            $currentTaskCount,
            $workloadStatus,
            $growthPlan
        );

        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue("$name", member.Name);
        command.Parameters.AddWithValue("$grade", member.Grade);
        command.Parameters.AddWithValue("$direction", member.Direction);
        command.Parameters.AddWithValue("$role", member.Role);
        command.Parameters.AddWithValue("$skillTags", member.SkillTags);
        command.Parameters.AddWithValue("$abilityLevel", member.AbilityLevel);
        command.Parameters.AddWithValue("$currentTaskCount", member.CurrentTaskCount);
        command.Parameters.AddWithValue("$workloadStatus", member.WorkloadStatus);
        command.Parameters.AddWithValue("$growthPlan", member.GrowthPlan);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public static void Update(MemberItem member)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE Members
        SET
            Name = $name,
            Grade = $grade,
            Direction = $direction,
            Role = $role,
            SkillTags = $skillTags,
            AbilityLevel = $abilityLevel,
            CurrentTaskCount = $currentTaskCount,
            WorkloadStatus = $workloadStatus,
            GrowthPlan = $growthPlan
        WHERE Id = $id;
        """;

        command.Parameters.AddWithValue("$id", member.Id);
        command.Parameters.AddWithValue("$name", member.Name);
        command.Parameters.AddWithValue("$grade", member.Grade);
        command.Parameters.AddWithValue("$direction", member.Direction);
        command.Parameters.AddWithValue("$role", member.Role);
        command.Parameters.AddWithValue("$skillTags", member.SkillTags);
        command.Parameters.AddWithValue("$abilityLevel", member.AbilityLevel);
        command.Parameters.AddWithValue("$currentTaskCount", member.CurrentTaskCount);
        command.Parameters.AddWithValue("$workloadStatus", member.WorkloadStatus);
        command.Parameters.AddWithValue("$growthPlan", member.GrowthPlan);

        command.ExecuteNonQuery();
    }

    public static void Delete(int memberId)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Members WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", memberId);

        command.ExecuteNonQuery();
    }
}