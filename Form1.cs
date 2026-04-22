using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CafeApp;

public sealed class Form1 : Form
{
    private const int TableCount = 10;

    private readonly Dictionary<int, Button> _tableButtons = new();
    private readonly Dictionary<int, TableInfo> _tableMap = new();

    private readonly FlowLayoutPanel _productsPanel = new();
    private readonly ListBox _ordersListBox = new();
    private readonly Label _selectedTableLabel = new();
    private readonly Label _totalPriceLabel = new();
    private readonly Button _openProductPageButton = new();
    private readonly Button _checkoutButton = new();

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
            RowCount = 7,
            Padding = new Padding(12),
            BackColor = Color.Gainsboro
        };

        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var productsTitle = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Products (click to add)",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Padding = new Padding(0, 0, 0, 6)
        };

        _productsPanel.Dock = DockStyle.Fill;
        _productsPanel.AutoScroll = true;
        _productsPanel.FlowDirection = FlowDirection.LeftToRight;
        _productsPanel.WrapContents = true;
        _productsPanel.Padding = new Padding(2);
        _productsPanel.BackColor = Color.White;

        _openProductPageButton.Text = "Open Add Product Page";
        _openProductPageButton.AutoSize = true;
        _openProductPageButton.Dock = DockStyle.Top;
        _openProductPageButton.BackColor = Color.FromArgb(60, 120, 200);
        _openProductPageButton.ForeColor = Color.White;
        _openProductPageButton.FlatStyle = FlatStyle.Flat;
        _openProductPageButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _openProductPageButton.Margin = new Padding(0, 8, 0, 0);
        _openProductPageButton.FlatAppearance.BorderSize = 0;
        _openProductPageButton.Click += OpenProductPageButtonOnClick;

        _selectedTableLabel.Dock = DockStyle.Fill;
        _selectedTableLabel.AutoSize = true;
        _selectedTableLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _selectedTableLabel.Padding = new Padding(0, 10, 0, 6);

        _ordersListBox.Dock = DockStyle.Fill;
        _ordersListBox.Font = new Font("Segoe UI", 9, FontStyle.Regular);
        _ordersListBox.IntegralHeight = false;

        _totalPriceLabel.Dock = DockStyle.Fill;
        _totalPriceLabel.AutoSize = true;
        _totalPriceLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        _totalPriceLabel.Padding = new Padding(0, 8, 0, 0);
        _totalPriceLabel.Text = "Total: 0.00 TL";

        _checkoutButton.Text = "Hesap Al";
        _checkoutButton.AutoSize = true;
        _checkoutButton.Dock = DockStyle.Top;
        _checkoutButton.BackColor = Color.FromArgb(189, 92, 37);
        _checkoutButton.ForeColor = Color.White;
        _checkoutButton.FlatStyle = FlatStyle.Flat;
        _checkoutButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _checkoutButton.Margin = new Padding(0, 8, 0, 0);
        _checkoutButton.FlatAppearance.BorderSize = 0;
        _checkoutButton.Click += CheckoutButtonOnClick;

        panel.Controls.Add(productsTitle, 0, 0);
        panel.Controls.Add(_productsPanel, 0, 1);
        panel.Controls.Add(_openProductPageButton, 0, 2);
        panel.Controls.Add(_selectedTableLabel, 0, 3);
        panel.Controls.Add(_ordersListBox, 0, 4);
        panel.Controls.Add(_totalPriceLabel, 0, 5);
        panel.Controls.Add(_checkoutButton, 0, 6);

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

        _productsPanel.SuspendLayout();
        _productsPanel.Controls.Clear();

        if (products.Count == 0)
        {
            _productsPanel.Controls.Add(new Label
            {
                Text = "No products. Use 'Open Add Product Page'.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.DimGray,
                Margin = new Padding(4)
            });

            _productsPanel.ResumeLayout();
            return;
        }

        var currentCategory = string.Empty;
        foreach (var product in products)
        {
            if (!string.Equals(currentCategory, product.Category, StringComparison.OrdinalIgnoreCase))
            {
                currentCategory = product.Category;

                var categoryLabel = new Label
                {
                    Text = currentCategory,
                    AutoSize = false,
                    Width = 320,
                    Height = 26,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(52, 52, 52),
                    BackColor = Color.FromArgb(235, 235, 235),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(8, 0, 0, 0),
                    Margin = new Padding(4, 8, 4, 4)
                };

                _productsPanel.Controls.Add(categoryLabel);
                _productsPanel.SetFlowBreak(categoryLabel, true);
            }

            var productButton = new Button
            {
                Width = 150,
                Height = 62,
                Text = $"{product.Name}\n{product.Price:0.00} TL",
                Tag = product,
                Margin = new Padding(4),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = GetProductButtonColor(product.Category),
                ForeColor = Color.White
            };

            productButton.FlatAppearance.BorderSize = 0;
            productButton.Click += ProductButtonOnClick;

            _productsPanel.Controls.Add(productButton);
        }

        _productsPanel.ResumeLayout();
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

        if (_selectedTableId == tableId)
        {
            button.FlatAppearance.BorderSize = 3;
            button.FlatAppearance.BorderColor = Color.Gold;
        }
        else
        {
            button.FlatAppearance.BorderSize = 0;
        }
    }

    private static Color GetProductButtonColor(string category)
    {
        var key = category.Trim().ToLowerInvariant();

        return key switch
        {
            "kahve" => Color.FromArgb(121, 85, 61),
            "cay" => Color.FromArgb(49, 132, 75),
            "soguk icecek" => Color.FromArgb(57, 118, 191),
            "tatli" => Color.FromArgb(171, 105, 57),
            "yiyecek" => Color.FromArgb(131, 98, 174),
            _ => Color.FromArgb(90, 103, 120)
        };
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

        _selectedTableId = tableId;

        var table = _tableMap[tableId];
        if (!table.IsOccupied)
        {
            table.IsOccupied = true;
            table.StartTime = DateTime.Now;
            DatabaseHelper.UpdateTableState(tableId, table.IsOccupied, table.StartTime);
        }

        RefreshTableButton(tableId);
        RefreshTableButtons();
        UpdateSelectedTableSection();
    }

    private void OpenProductPageButtonOnClick(object? sender, EventArgs e)
    {
        var existingCategories = DatabaseHelper.GetProductCategories();
        using var addProductForm = new AddProductForm(existingCategories);
        if (addProductForm.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        DatabaseHelper.AddProduct(addProductForm.ProductCategory, addProductForm.ProductName, addProductForm.ProductPrice);
        LoadProducts();
    }

    private void ProductButtonOnClick(object? sender, EventArgs e)
    {
        if (_selectedTableId <= 0 || !_tableMap.ContainsKey(_selectedTableId))
        {
            MessageBox.Show("Please click a table first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedTable = _tableMap[_selectedTableId];
        if (!selectedTable.IsOccupied)
        {
            MessageBox.Show("This table is available. Open it first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (sender is not Button clickedButton || clickedButton.Tag is not ProductInfo product)
        {
            return;
        }

        DatabaseHelper.AddOrder(_selectedTableId, product.Name, product.Price);
        UpdateSelectedTableSection();
    }

    private void CheckoutButtonOnClick(object? sender, EventArgs e)
    {
        if (_selectedTableId <= 0 || !_tableMap.ContainsKey(_selectedTableId))
        {
            MessageBox.Show("Please select a table first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var table = _tableMap[_selectedTableId];
        var total = DatabaseHelper.GetTotalForTable(_selectedTableId);
        var orderCount = DatabaseHelper.GetOrderCountForTable(_selectedTableId);

        if (orderCount == 0)
        {
            if (!table.IsOccupied)
            {
                MessageBox.Show("No active order for this table.", "Hesap Al", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var closeEmpty = MessageBox.Show(
                "There is no order on this table. Close table?",
                "Hesap Al",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (closeEmpty == DialogResult.Yes)
            {
                table.IsOccupied = false;
                table.StartTime = null;
                DatabaseHelper.UpdateTableState(_selectedTableId, false, null);
                RefreshTableButtons();
                UpdateSelectedTableSection();
            }

            return;
        }

        MessageBox.Show(
            $"Table {_selectedTableId} total bill: {total:0.00} TL",
            "Hesap Al",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        DatabaseHelper.ClearOrdersForTable(_selectedTableId);
        table.IsOccupied = false;
        table.StartTime = null;
        DatabaseHelper.UpdateTableState(_selectedTableId, false, null);

        RefreshTableButtons();
        UpdateSelectedTableSection();
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