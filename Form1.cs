using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CafeApp;

public sealed class Form1 : Form
{
    private const int TableCount = 10;

    private readonly Dictionary<int, Button> _tableButtons = new();
    private readonly Dictionary<int, TableInfo> _tableMap = new();

    private readonly ListBox _productsListBox = new();
    private readonly ListBox _ordersListBox = new();
    private readonly TextBox _productNameTextBox = new();
    private readonly TextBox _productPriceTextBox = new();
    private readonly Label _selectedTableLabel = new();
    private readonly Label _totalPriceLabel = new();

    private readonly Timer _uiTimer = new();

    private int _selectedTableId;

    public Form1()
    {
        ConfigureForm();
        CreateLayout();

        DatabaseHelper.InitializeDatabase();
        LoadTables();
        LoadProducts();
        UpdateSelectedTableSection();

        _uiTimer.Interval = 1000;
        _uiTimer.Tick += UiTimerOnTick;
        _uiTimer.Start();
    }

    private void ConfigureForm()
    {
        Text = "CafeApp - Cafe Management";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1150;
        Height = 700;
        MinimumSize = new Size(1000, 620);
    }

    private void CreateLayout()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.WhiteSmoke
        };

        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));

        Controls.Add(mainLayout);

        var tablesGrid = BuildTablesGrid();
        var rightPanel = BuildRightPanel();

        mainLayout.Controls.Add(tablesGrid, 0, 0);
        mainLayout.Controls.Add(rightPanel, 1, 0);
    }

    private TableLayoutPanel BuildTablesGrid()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(18),
            BackColor = Color.WhiteSmoke
        };

        for (var col = 0; col < 2; col++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        }

        for (var row = 0; row < 5; row++)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        }

        for (var tableId = 1; tableId <= TableCount; tableId++)
        {
            var tableButton = new Button
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Tag = tableId,
                Text = $"Table {tableId}"
            };

            tableButton.FlatAppearance.BorderSize = 0;
            tableButton.Click += TableButtonOnClick;

            _tableButtons[tableId] = tableButton;

            var row = (tableId - 1) / 2;
            var col = (tableId - 1) % 2;
            grid.Controls.Add(tableButton, col, row);
        }

        return grid;
    }

    private TableLayoutPanel BuildRightPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 12,
            Padding = new Padding(12),
            BackColor = Color.Gainsboro
        };

        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var productsTitle = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Products",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Padding = new Padding(0, 0, 0, 6)
        };

        _productsListBox.Dock = DockStyle.Fill;
        _productsListBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var addProductTitle = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Add Product",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Padding = new Padding(0, 8, 0, 6)
        };

        var nameLabel = new Label
        {
            Text = "Name:",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };

        _productNameTextBox.Dock = DockStyle.Top;
        _productNameTextBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var priceLabel = new Label
        {
            Text = "Price:",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            Padding = new Padding(0, 6, 0, 0)
        };

        _productPriceTextBox.Dock = DockStyle.Top;
        _productPriceTextBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var addProductButton = new Button
        {
            Text = "Add Product",
            AutoSize = true,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(60, 120, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(0, 8, 0, 0)
        };

        addProductButton.FlatAppearance.BorderSize = 0;
        addProductButton.Click += AddProductButtonOnClick;

        _selectedTableLabel.Dock = DockStyle.Fill;
        _selectedTableLabel.AutoSize = true;
        _selectedTableLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _selectedTableLabel.Padding = new Padding(0, 12, 0, 6);

        _ordersListBox.Dock = DockStyle.Fill;
        _ordersListBox.Font = new Font("Segoe UI", 9, FontStyle.Regular);
        _ordersListBox.IntegralHeight = false;

        var addOrderButton = new Button
        {
            Text = "Add Selected Product To Table",
            AutoSize = true,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(32, 153, 95),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(0, 8, 0, 0)
        };

        addOrderButton.FlatAppearance.BorderSize = 0;
        addOrderButton.Click += AddOrderButtonOnClick;

        _totalPriceLabel.Dock = DockStyle.Fill;
        _totalPriceLabel.AutoSize = true;
        _totalPriceLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        _totalPriceLabel.Padding = new Padding(0, 8, 0, 0);
        _totalPriceLabel.Text = "Total: 0.00 TL";

        panel.Controls.Add(productsTitle, 0, 0);
        panel.Controls.Add(_productsListBox, 0, 1);
        panel.Controls.Add(addProductTitle, 0, 2);
        panel.Controls.Add(nameLabel, 0, 3);
        panel.Controls.Add(_productNameTextBox, 0, 4);
        panel.Controls.Add(priceLabel, 0, 5);
        panel.Controls.Add(_productPriceTextBox, 0, 6);
        panel.Controls.Add(addProductButton, 0, 7);
        panel.Controls.Add(_selectedTableLabel, 0, 8);
        panel.Controls.Add(_ordersListBox, 0, 9);
        panel.Controls.Add(addOrderButton, 0, 10);
        panel.Controls.Add(_totalPriceLabel, 0, 11);

        return panel;
    }

    private void LoadTables()
    {
        _tableMap.Clear();

        var allTables = DatabaseHelper.GetTables();
        foreach (var table in allTables)
        {
            _tableMap[table.Id] = table;
        }

        for (var i = 1; i <= TableCount; i++)
        {
            if (!_tableMap.ContainsKey(i))
            {
                _tableMap[i] = new TableInfo
                {
                    Id = i,
                    Name = $"Table {i}",
                    IsOccupied = false,
                    StartTime = null
                };
            }
        }

        RefreshTableButtons();
    }

    private void LoadProducts()
    {
        var products = DatabaseHelper.GetProducts();
        _productsListBox.DataSource = null;
        _productsListBox.DataSource = products;
    }

    private void RefreshTableButtons()
    {
        for (var tableId = 1; tableId <= TableCount; tableId++)
        {
            RefreshTableButton(tableId);
        }
    }

    private void RefreshTableButton(int tableId)
    {
        if (!_tableButtons.ContainsKey(tableId) || !_tableMap.ContainsKey(tableId))
        {
            return;
        }

        var button = _tableButtons[tableId];
        var table = _tableMap[tableId];

        if (table.IsOccupied)
        {
            var elapsedMinutes = GetElapsedMinutes(table.StartTime);
            button.BackColor = Color.IndianRed;
            button.Text = $"Table {tableId}\nOccupied\n{elapsedMinutes} min";
        }
        else
        {
            button.BackColor = Color.MediumSeaGreen;
            button.Text = $"Table {tableId}\nAvailable";
        }
    }

    private static int GetElapsedMinutes(DateTime? startTime)
    {
        if (!startTime.HasValue)
        {
            return 0;
        }

        var elapsed = DateTime.Now - startTime.Value;
        if (elapsed.TotalMinutes < 0)
        {
            return 0;
        }

        return (int)elapsed.TotalMinutes;
    }

    private void TableButtonOnClick(object? sender, EventArgs e)
    {
        if (sender is not Button clickedButton || clickedButton.Tag is not int tableId)
        {
            return;
        }

        if (!_tableMap.ContainsKey(tableId))
        {
            return;
        }

        var table = _tableMap[tableId];
        table.IsOccupied = !table.IsOccupied;

        if (table.IsOccupied)
        {
            table.StartTime = DateTime.Now;
        }
        else
        {
            table.StartTime = null;
            DatabaseHelper.ClearOrdersForTable(tableId);
        }

        DatabaseHelper.UpdateTableState(tableId, table.IsOccupied, table.StartTime);

        _selectedTableId = tableId;

        RefreshTableButton(tableId);
        UpdateSelectedTableSection();
    }

    private void AddProductButtonOnClick(object? sender, EventArgs e)
    {
        var name = _productNameTextBox.Text.Trim();
        var priceText = _productPriceTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a product name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!TryParsePrice(priceText, out var price) || price <= 0)
        {
            MessageBox.Show("Please enter a valid price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DatabaseHelper.AddProduct(name, price);
        _productNameTextBox.Clear();
        _productPriceTextBox.Clear();

        LoadProducts();
    }

    private void AddOrderButtonOnClick(object? sender, EventArgs e)
    {
        if (_selectedTableId <= 0 || !_tableMap.ContainsKey(_selectedTableId))
        {
            MessageBox.Show("Please click a table first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedTable = _tableMap[_selectedTableId];
        if (!selectedTable.IsOccupied)
        {
            MessageBox.Show("This table is available. Occupy it first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_productsListBox.SelectedItem is not ProductInfo product)
        {
            MessageBox.Show("Please select a product from the list.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DatabaseHelper.AddOrder(_selectedTableId, product.Name, product.Price);
        UpdateSelectedTableSection();
    }

    private static bool TryParsePrice(string value, out decimal price)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out price))
        {
            return true;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out price);
    }

    private void UpdateSelectedTableSection()
    {
        if (_selectedTableId <= 0 || !_tableMap.ContainsKey(_selectedTableId))
        {
            _selectedTableLabel.Text = "Selected Table: None";
            _ordersListBox.Items.Clear();
            _totalPriceLabel.Text = "Total: 0.00 TL";
            return;
        }

        var table = _tableMap[_selectedTableId];
        var stateText = table.IsOccupied ? "Occupied" : "Available";
        var minuteText = table.IsOccupied ? $", Elapsed: {GetElapsedMinutes(table.StartTime)} min" : string.Empty;
        _selectedTableLabel.Text = $"Selected Table: {_selectedTableId} ({stateText}{minuteText})";

        _ordersListBox.Items.Clear();
        var orders = DatabaseHelper.GetOrdersForTable(_selectedTableId);
        foreach (var order in orders)
        {
            _ordersListBox.Items.Add($"{order.ProductName} - {order.Price:0.00} TL");
        }

        var total = DatabaseHelper.GetTotalForTable(_selectedTableId);
        _totalPriceLabel.Text = $"Total: {total:0.00} TL";
    }

    private void UiTimerOnTick(object? sender, EventArgs e)
    {
        RefreshTableButtons();

        if (_selectedTableId > 0)
        {
            UpdateSelectedTableSection();
        }
    }
}