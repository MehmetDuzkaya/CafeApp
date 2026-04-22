using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace CafeApp;

internal static class DatabaseHelper
{
    private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cafe.db");
    private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

    public static void InitializeDatabase()
    {
        if (!File.Exists(DbPath))
        {
            SQLiteConnection.CreateFile(DbPath);
        }

        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using (var command = new SQLiteCommand(connection))
        {
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS cafe_tables (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    is_occupied INTEGER NOT NULL DEFAULT 0,
    start_time TEXT NULL
);

CREATE TABLE IF NOT EXISTS products (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    price REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS orders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    table_id INTEGER NOT NULL,
    product_name TEXT NOT NULL,
    price REAL NOT NULL
);";

            command.ExecuteNonQuery();
        }

        EnsureCafeTables(connection);
    }

    private static void EnsureCafeTables(SQLiteConnection connection)
    {
        for (var i = 1; i <= 10; i++)
        {
            using var command = new SQLiteCommand(
                @"INSERT OR IGNORE INTO cafe_tables (id, name, is_occupied, start_time)
                  VALUES (@id, @name, 0, NULL);",
                connection);

            command.Parameters.AddWithValue("@id", i);
            command.Parameters.AddWithValue("@name", $"Table {i}");
            command.ExecuteNonQuery();
        }
    }

    public static List<TableInfo> GetTables()
    {
        var tables = new List<TableInfo>();

        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "SELECT id, name, is_occupied, start_time FROM cafe_tables ORDER BY id;",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(new TableInfo
            {
                Id = Convert.ToInt32(reader["id"]),
                Name = Convert.ToString(reader["name"]) ?? string.Empty,
                IsOccupied = Convert.ToInt32(reader["is_occupied"]) == 1,
                StartTime = ParseNullableDateTime(reader["start_time"])
            });
        }

        return tables;
    }

    public static void UpdateTableState(int tableId, bool isOccupied, DateTime? startTime)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            @"UPDATE cafe_tables
              SET is_occupied = @is_occupied,
                  start_time = @start_time
              WHERE id = @id;",
            connection);

        command.Parameters.AddWithValue("@id", tableId);
        command.Parameters.AddWithValue("@is_occupied", isOccupied ? 1 : 0);
        command.Parameters.AddWithValue(
            "@start_time",
            startTime.HasValue
                ? startTime.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                : DBNull.Value);

        command.ExecuteNonQuery();
    }

    public static void AddProduct(string name, decimal price)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "INSERT INTO products (name, price) VALUES (@name, @price);",
            connection);

        command.Parameters.AddWithValue("@name", name.Trim());
        command.Parameters.AddWithValue("@price", (double)price);
        command.ExecuteNonQuery();
    }

    public static List<ProductInfo> GetProducts()
    {
        var products = new List<ProductInfo>();

        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "SELECT id, name, price FROM products ORDER BY name;",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            products.Add(new ProductInfo
            {
                Id = Convert.ToInt32(reader["id"]),
                Name = Convert.ToString(reader["name"]) ?? string.Empty,
                Price = Convert.ToDecimal(reader["price"])
            });
        }

        return products;
    }

    public static void AddOrder(int tableId, string productName, decimal price)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "INSERT INTO orders (table_id, product_name, price) VALUES (@table_id, @product_name, @price);",
            connection);

        command.Parameters.AddWithValue("@table_id", tableId);
        command.Parameters.AddWithValue("@product_name", productName);
        command.Parameters.AddWithValue("@price", (double)price);
        command.ExecuteNonQuery();
    }

    public static List<OrderInfo> GetOrdersForTable(int tableId)
    {
        var orders = new List<OrderInfo>();

        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            @"SELECT id, table_id, product_name, price
              FROM orders
              WHERE table_id = @table_id
              ORDER BY id;",
            connection);

        command.Parameters.AddWithValue("@table_id", tableId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            orders.Add(new OrderInfo
            {
                Id = Convert.ToInt32(reader["id"]),
                TableId = Convert.ToInt32(reader["table_id"]),
                ProductName = Convert.ToString(reader["product_name"]) ?? string.Empty,
                Price = Convert.ToDecimal(reader["price"])
            });
        }

        return orders;
    }

    public static decimal GetTotalForTable(int tableId)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "SELECT IFNULL(SUM(price), 0) FROM orders WHERE table_id = @table_id;",
            connection);

        command.Parameters.AddWithValue("@table_id", tableId);

        var result = command.ExecuteScalar();
        return result == null || result == DBNull.Value
            ? 0m
            : Convert.ToDecimal(result);
    }

    public static void ClearOrdersForTable(int tableId)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "DELETE FROM orders WHERE table_id = @table_id;",
            connection);

        command.Parameters.AddWithValue("@table_id", tableId);
        command.ExecuteNonQuery();
    }

    private static DateTime? ParseNullableDateTime(object rawValue)
    {
        if (rawValue == DBNull.Value)
        {
            return null;
        }

        var textValue = Convert.ToString(rawValue);
        if (string.IsNullOrWhiteSpace(textValue))
        {
            return null;
        }

        if (DateTime.TryParse(textValue, out var parsed))
        {
            return parsed;
        }

        return null;
    }
}

internal sealed class TableInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
    public DateTime? StartTime { get; set; }
}

internal sealed class ProductInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public override string ToString()
    {
        return $"{Name} - {Price:0.00} TL";
    }
}

internal sealed class OrderInfo
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}