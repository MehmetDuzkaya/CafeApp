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
    category TEXT NOT NULL DEFAULT 'Genel',
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

        EnsureProductCategoryColumn(connection);
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

    private static void EnsureProductCategoryColumn(SQLiteConnection connection)
    {
        var hasCategoryColumn = false;

        using (var command = new SQLiteCommand("PRAGMA table_info(products);", connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var columnName = Convert.ToString(reader["name"]);
                if (string.Equals(columnName, "category", StringComparison.OrdinalIgnoreCase))
                {
                    hasCategoryColumn = true;
                    break;
                }
            }
        }

        if (!hasCategoryColumn)
        {
            using var alterCommand = new SQLiteCommand(
                "ALTER TABLE products ADD COLUMN category TEXT NOT NULL DEFAULT 'Genel';",
                connection);
            alterCommand.ExecuteNonQuery();
        }

        using var fillCommand = new SQLiteCommand(
            "UPDATE products SET category = 'Genel' WHERE category IS NULL OR TRIM(category) = '';",
            connection);
        fillCommand.ExecuteNonQuery();
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

    public static void AddProduct(string category, string name, decimal price)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "INSERT INTO products (category, name, price) VALUES (@category, @name, @price);",
            connection);

        command.Parameters.AddWithValue("@category", category.Trim());
        command.Parameters.AddWithValue("@name", name.Trim());
        command.Parameters.AddWithValue("@price", (double)price);
        command.ExecuteNonQuery();
    }

    public static void UpdateProduct(int productId, string category, string name, decimal price)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            @"UPDATE products
              SET category = @category,
                  name = @name,
                  price = @price
              WHERE id = @id;",
            connection);

        command.Parameters.AddWithValue("@id", productId);
        command.Parameters.AddWithValue("@category", category.Trim());
        command.Parameters.AddWithValue("@name", name.Trim());
        command.Parameters.AddWithValue("@price", (double)price);
        command.ExecuteNonQuery();
    }

    public static void DeleteProduct(int productId)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "DELETE FROM products WHERE id = @id;",
            connection);

        command.Parameters.AddWithValue("@id", productId);
        command.ExecuteNonQuery();
    }

    public static List<string> GetProductCategories()
    {
        var categories = new List<string>();

        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            @"SELECT DISTINCT category
              FROM products
              WHERE category IS NOT NULL AND TRIM(category) <> ''
              ORDER BY category;",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(Convert.ToString(reader["category"]) ?? string.Empty);
        }

        return categories;
    }

    public static List<ProductInfo> GetProducts()
    {
        var products = new List<ProductInfo>();

        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "SELECT id, category, name, price FROM products ORDER BY category, name;",
            connection);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            products.Add(new ProductInfo
            {
                Id = Convert.ToInt32(reader["id"]),
                Category = Convert.ToString(reader["category"]) ?? "Genel",
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

    public static int GetOrderCountForTable(int tableId)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "SELECT COUNT(1) FROM orders WHERE table_id = @table_id;",
            connection);

        command.Parameters.AddWithValue("@table_id", tableId);

        var result = command.ExecuteScalar();
        return result == null || result == DBNull.Value
            ? 0
            : Convert.ToInt32(result);
    }

    public static void DeleteOrder(int orderId)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        using var command = new SQLiteCommand(
            "DELETE FROM orders WHERE id = @id;",
            connection);

        command.Parameters.AddWithValue("@id", orderId);
        command.ExecuteNonQuery();
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
    public string Category { get; set; } = "Genel";
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public override string ToString()
    {
        return $"[{Category}] {Name} - {Price:0.00} TL";
    }
}

internal sealed class OrderInfo
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public override string ToString()
    {
        return $"{ProductName} - {Price:0.00} TL";
    }
}