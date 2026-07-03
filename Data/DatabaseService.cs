using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace TeamFlowDesk.Data;

public static class DatabaseService
{
    private const string DatabaseFileName = "teamflowdesk.db";

    public static string DatabasePath
    {
        get
        {
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TeamFlowDesk");

            Directory.CreateDirectory(appDataFolder);

            return Path.Combine(appDataFolder, DatabaseFileName);
        }
    }

    public static string ConnectionString => $"Data Source={DatabasePath}";

    public static void InitializeDatabase()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        CreateTasksTable(connection);
        CreateEquipmentTable(connection);
        CreateMembersTable(connection);
    }

    private static void CreateTasksTable(SqliteConnection connection)
    {
        var command = connection.CreateCommand();

        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS Tasks (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ProjectId INTEGER NOT NULL,
            Title TEXT NOT NULL,
            Description TEXT NOT NULL,
            OwnerName TEXT NOT NULL,
            Collaborators TEXT NOT NULL,
            Status TEXT NOT NULL,
            Priority TEXT NOT NULL,
            RiskLevel TEXT NOT NULL,
            Deadline TEXT NOT NULL,
            RelatedEquipment TEXT NOT NULL,
            OutputRequirement TEXT NOT NULL
        );
        """;

        command.ExecuteNonQuery();
    }

    private static void CreateEquipmentTable(SqliteConnection connection)
    {
        var command = connection.CreateCommand();

        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS Equipment (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Code TEXT NOT NULL,
            Category TEXT NOT NULL,
            Status TEXT NOT NULL,
            Location TEXT NOT NULL,
            CurrentHolder TEXT NOT NULL,
            RelatedTask TEXT NOT NULL,
            MaintenanceRecord TEXT NOT NULL
        );
        """;

        command.ExecuteNonQuery();
    }

    private static void CreateMembersTable(SqliteConnection connection)
    {
        var command = connection.CreateCommand();

        command.CommandText =
        """
        CREATE TABLE IF NOT EXISTS Members (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Grade TEXT NOT NULL,
            Direction TEXT NOT NULL,
            Role TEXT NOT NULL,
            SkillTags TEXT NOT NULL,
            AbilityLevel TEXT NOT NULL,
            CurrentTaskCount INTEGER NOT NULL,
            WorkloadStatus TEXT NOT NULL,
            GrowthPlan TEXT NOT NULL
        );
        """;

        command.ExecuteNonQuery();
    }
}