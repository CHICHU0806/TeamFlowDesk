using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Data;

public static class EquipmentRepository
{
    public static List<EquipmentItem> GetAll()
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
            Code,
            Category,
            Status,
            Location,
            CurrentHolder,
            RelatedTask,
            MaintenanceRecord
        FROM Equipment
        ORDER BY Id DESC;
        """;

        var equipment = new List<EquipmentItem>();

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            equipment.Add(new EquipmentItem
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Code = reader.GetString(2),
                Category = reader.GetString(3),
                Status = reader.GetString(4),
                Location = reader.GetString(5),
                CurrentHolder = reader.GetString(6),
                RelatedTask = reader.GetString(7),
                MaintenanceRecord = reader.GetString(8)
            });
        }

        return equipment;
    }

    public static void SeedIfEmpty()
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Equipment;";

        var count = Convert.ToInt32(countCommand.ExecuteScalar());

        if (count > 0)
        {
            return;
        }

        foreach (var item in MockDataService.GetEquipment())
        {
            Add(item);
        }
    }

    public static int Add(EquipmentItem item)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        INSERT INTO Equipment (
            Name,
            Code,
            Category,
            Status,
            Location,
            CurrentHolder,
            RelatedTask,
            MaintenanceRecord
        )
        VALUES (
            $name,
            $code,
            $category,
            $status,
            $location,
            $currentHolder,
            $relatedTask,
            $maintenanceRecord
        );

        SELECT last_insert_rowid();
        """;

        command.Parameters.AddWithValue("$name", item.Name);
        command.Parameters.AddWithValue("$code", item.Code);
        command.Parameters.AddWithValue("$category", item.Category);
        command.Parameters.AddWithValue("$status", item.Status);
        command.Parameters.AddWithValue("$location", item.Location);
        command.Parameters.AddWithValue("$currentHolder", item.CurrentHolder);
        command.Parameters.AddWithValue("$relatedTask", item.RelatedTask);
        command.Parameters.AddWithValue("$maintenanceRecord", item.MaintenanceRecord);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public static void Update(EquipmentItem item)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText =
        """
        UPDATE Equipment
        SET
            Name = $name,
            Code = $code,
            Category = $category,
            Status = $status,
            Location = $location,
            CurrentHolder = $currentHolder,
            RelatedTask = $relatedTask,
            MaintenanceRecord = $maintenanceRecord
        WHERE Id = $id;
        """;

        command.Parameters.AddWithValue("$id", item.Id);
        command.Parameters.AddWithValue("$name", item.Name);
        command.Parameters.AddWithValue("$code", item.Code);
        command.Parameters.AddWithValue("$category", item.Category);
        command.Parameters.AddWithValue("$status", item.Status);
        command.Parameters.AddWithValue("$location", item.Location);
        command.Parameters.AddWithValue("$currentHolder", item.CurrentHolder);
        command.Parameters.AddWithValue("$relatedTask", item.RelatedTask);
        command.Parameters.AddWithValue("$maintenanceRecord", item.MaintenanceRecord);

        command.ExecuteNonQuery();
    }

    public static void Delete(int equipmentId)
    {
        DatabaseService.InitializeDatabase();

        using var connection = new SqliteConnection(DatabaseService.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM Equipment WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", equipmentId);

        command.ExecuteNonQuery();
    }
}