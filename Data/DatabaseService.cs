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
}