using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CafeApp;

public sealed class Form1 : Form
{
    private readonly Dictionary<int, Button> _tableButtons = new();
    private readonly Dictionary<int, TableInfo> _tableMap = new();

    private TableLayoutPanel _tablesGrid = new();
    private readonly FlowLayoutPanel _productsPanel = new();
    private readonly ListBox _ordersListBox = new();
    private readonly Label _selectedTableLabel = new();
    private readonly Label _totalPriceLabel = new();
    private readonly Button _openProductPageButton = new();
    private readonly Button _removeOrderItemButton = new();
    private readonly Button _checkoutButton = new();
    private readonly Button _addTableButton = new();
    private readonly Button _removeTableButton = new();
    private readonly Button _renameTableButton = new();

    private readonly System.Windows.Forms.Timer _uiTimer = new();

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
        Text = "CafeApp - Kafe Yonetimi";
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

        _tablesGrid = BuildTablesGrid();
        var rightPanel = BuildRightPanel();

        mainLayout.Controls.Add(_tablesGrid, 0, 0);
        mainLayout.Controls.Add(rightPanel, 1, 0);
    }

    private TableLayoutPanel BuildTablesGrid()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(18),
            BackColor = Color.WhiteSmoke
        };

        for (var col = 0; col < 2; col++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        }

        return grid;
    }

    private TableLayoutPanel BuildRightPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(12),
            BackColor = Color.Gainsboro
        };

        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 72f));

        var productsTitle = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Urunler (eklemek icin tikla)",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Padding = new Padding(0, 0, 0, 6)
        };

        _productsPanel.Dock = DockStyle.Fill;
        _productsPanel.AutoScroll = true;
        _productsPanel.FlowDirection = FlowDirection.LeftToRight;
        _productsPanel.WrapContents = true;
        _productsPanel.Padding = new Padding(2);
        _productsPanel.BackColor = Color.White;

        _openProductPageButton.Text = "Urun Yonetimi";
        _openProductPageButton.AutoSize = false;
        _openProductPageButton.Dock = DockStyle.Fill;
        _openProductPageButton.BackColor = Color.FromArgb(60, 120, 200);
        _openProductPageButton.ForeColor = Color.White;
        _openProductPageButton.FlatStyle = FlatStyle.Flat;
        _openProductPageButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _openProductPageButton.Margin = new Padding(0, 0, 6, 0);
        _openProductPageButton.FlatAppearance.BorderSize = 0;
        _openProductPageButton.Click += OpenProductPageButtonOnClick;

        _selectedTableLabel.Dock = DockStyle.Fill;
        _selectedTableLabel.AutoSize = true;
        _selectedTableLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _selectedTableLabel.Padding = new Padding(0, 10, 0, 6);

        _ordersListBox.Dock = DockStyle.Fill;
        _ordersListBox.Font = new Font("Segoe UI", 9, FontStyle.Regular);
        _ordersListBox.IntegralHeight = false;

        _removeOrderItemButton.Text = "Secili Urunu Siparisten Sil";
        _removeOrderItemButton.AutoSize = false;
        _removeOrderItemButton.Dock = DockStyle.Fill;
        _removeOrderItemButton.BackColor = Color.FromArgb(150, 63, 63);
        _removeOrderItemButton.ForeColor = Color.White;
        _removeOrderItemButton.FlatStyle = FlatStyle.Flat;
        _removeOrderItemButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _removeOrderItemButton.Margin = new Padding(6, 0, 0, 0);
        _removeOrderItemButton.FlatAppearance.BorderSize = 0;
        _removeOrderItemButton.Click += RemoveOrderItemButtonOnClick;

        _totalPriceLabel.Dock = DockStyle.Fill;
        _totalPriceLabel.AutoSize = true;
        _totalPriceLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        _totalPriceLabel.Padding = new Padding(0, 8, 0, 0);
        _totalPriceLabel.Text = "Toplam: 0.00 TL";

        _checkoutButton.Text = "Hesap Al";
        _checkoutButton.AutoSize = false;
        _checkoutButton.Dock = DockStyle.Fill;
        _checkoutButton.BackColor = Color.FromArgb(189, 92, 37);
        _checkoutButton.ForeColor = Color.White;
        _checkoutButton.FlatStyle = FlatStyle.Flat;
        _checkoutButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        _checkoutButton.Margin = new Padding(0, 12, 0, 0);
        _checkoutButton.FlatAppearance.BorderSize = 0;
        _checkoutButton.Click += CheckoutButtonOnClick;

        var actionButtonsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            Height = 42,
            Margin = new Padding(0, 8, 0, 0)
        };

        actionButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        actionButtonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        actionButtonsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));

        actionButtonsPanel.Controls.Add(_openProductPageButton, 0, 0);
        actionButtonsPanel.Controls.Add(_removeOrderItemButton, 1, 0);

        var tableActionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 6)
        };

        _addTableButton.Text = "Masa Ekle";
        _addTableButton.AutoSize = true;
        _addTableButton.BackColor = Color.FromArgb(32, 153, 95);
        _addTableButton.ForeColor = Color.White;
        _addTableButton.FlatStyle = FlatStyle.Flat;
        _addTableButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _addTableButton.FlatAppearance.BorderSize = 0;
        _addTableButton.Margin = new Padding(0, 0, 8, 0);
        _addTableButton.Click += AddTableButtonOnClick;

        _removeTableButton.Text = "Masa Sil";
        _removeTableButton.AutoSize = true;
        _removeTableButton.BackColor = Color.FromArgb(193, 62, 62);
        _removeTableButton.ForeColor = Color.White;
        _removeTableButton.FlatStyle = FlatStyle.Flat;
        _removeTableButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _removeTableButton.FlatAppearance.BorderSize = 0;
        _removeTableButton.Margin = new Padding(0, 0, 8, 0);
        _removeTableButton.Click += RemoveTableButtonOnClick;

        _renameTableButton.Text = "Masa Adi Duzenle";
        _renameTableButton.AutoSize = true;
        _renameTableButton.BackColor = Color.FromArgb(60, 120, 200);
        _renameTableButton.ForeColor = Color.White;
        _renameTableButton.FlatStyle = FlatStyle.Flat;
        _renameTableButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _renameTableButton.FlatAppearance.BorderSize = 0;
        _renameTableButton.Margin = new Padding(0, 0, 8, 0);
        _renameTableButton.Click += RenameTableButtonOnClick;

        tableActionsPanel.Controls.Add(_addTableButton);
        tableActionsPanel.Controls.Add(_removeTableButton);
        tableActionsPanel.Controls.Add(_renameTableButton);

        panel.Controls.Add(productsTitle, 0, 0);
        panel.Controls.Add(_productsPanel, 0, 1);
        panel.Controls.Add(actionButtonsPanel, 0, 2);
        panel.Controls.Add(_selectedTableLabel, 0, 3);
        panel.Controls.Add(tableActionsPanel, 0, 4);
        panel.Controls.Add(_ordersListBox, 0, 5);
        panel.Controls.Add(_totalPriceLabel, 0, 6);
        panel.Controls.Add(_checkoutButton, 0, 7);

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

        if (!_tableMap.ContainsKey(_selectedTableId))
        {
            _selectedTableId = 0;
        }

        RenderTableButtons();
        RefreshTableButtons();
    }

    private void RenderTableButtons()
    {
        _tablesGrid.SuspendLayout();
        _tablesGrid.Controls.Clear();
        _tablesGrid.RowStyles.Clear();
        _tableButtons.Clear();

        var orderedTables = new List<TableInfo>(_tableMap.Values);
        orderedTables.Sort((left, right) => left.Id.CompareTo(right.Id));

        if (orderedTables.Count == 0)
        {
            _tablesGrid.RowCount = 1;
            _tablesGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            _tablesGrid.ResumeLayout();
            return;
        }

        var rows = (int)Math.Ceiling(orderedTables.Count / 2.0);
        _tablesGrid.RowCount = rows;

        for (var row = 0; row < rows; row++)
        {
            _tablesGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
        }

        for (var index = 0; index < orderedTables.Count; index++)
        {
            var table = orderedTables[index];
            var tableButton = new Button
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Tag = table.Id,
                Text = table.Name
            };

            tableButton.FlatAppearance.BorderSize = 0;
            tableButton.Click += TableButtonOnClick;

            _tableButtons[table.Id] = tableButton;

            var row = index / 2;
            var col = index % 2;
            _tablesGrid.Controls.Add(tableButton, col, row);
        }

        _tablesGrid.ResumeLayout();
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
                Text = "Urun yok. 'Urun Yonetimi' ile ekleyin.",
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
        foreach (var tableId in _tableMap.Keys)
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
            button.Text = $"{table.Name}\nDolu\n{elapsedMinutes} dk";
        }
        else
        {
            button.BackColor = Color.MediumSeaGreen;
            button.Text = $"{table.Name}\nMusait";
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
            "icecekler" => Color.FromArgb(57, 118, 191),
            "kahve" => Color.FromArgb(57, 118, 191),
            "cay" => Color.FromArgb(57, 118, 191),
            "soguk icecek" => Color.FromArgb(57, 118, 191),
            "baklava" => Color.FromArgb(179, 121, 72),
            "tatli" => Color.FromArgb(171, 105, 57),
            "soguk tatli" => Color.FromArgb(96, 142, 187),
            "dondurma" => Color.FromArgb(120, 162, 214),
            "kurabiye" => Color.FromArgb(160, 122, 85),
            "borek" => Color.FromArgb(191, 133, 72),
            "yiyecek" => Color.FromArgb(131, 98, 174),
            "genel" => Color.FromArgb(90, 103, 120),
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
        using var addProductForm = new AddProductForm();
        addProductForm.ShowDialog(this);

        if (addProductForm.HasChanges)
        {
            LoadProducts();
        }
    }

    private void ProductButtonOnClick(object? sender, EventArgs e)
    {
        if (_selectedTableId <= 0 || !_tableMap.ContainsKey(_selectedTableId))
        {
            MessageBox.Show("Once masa secin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selectedTable = _tableMap[_selectedTableId];
        if (!selectedTable.IsOccupied)
        {
            MessageBox.Show("Bu masa musait. Once masayi acin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (sender is not Button clickedButton || clickedButton.Tag is not ProductInfo product)
        {
            return;
        }

        DatabaseHelper.AddOrder(_selectedTableId, product.Name, product.Price);
        UpdateSelectedTableSection();
    }

    private void RemoveOrderItemButtonOnClick(object? sender, EventArgs e)
    {
        if (_selectedTableId <= 0 || !_tableMap.ContainsKey(_selectedTableId))
        {
            MessageBox.Show("Once masa secin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_ordersListBox.SelectedItem is not OrderInfo selectedOrder)
        {
            MessageBox.Show("Listeden bir siparis secin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DatabaseHelper.DeleteOrder(selectedOrder.Id);
        UpdateSelectedTableSection();
    }

    private void CheckoutButtonOnClick(object? sender, EventArgs e)
    {
        if (_selectedTableId <= 0 || !_tableMap.ContainsKey(_selectedTableId))
        {
            MessageBox.Show("Once masa secin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var table = _tableMap[_selectedTableId];
        var total = DatabaseHelper.GetTotalForTable(_selectedTableId);
        var orderCount = DatabaseHelper.GetOrderCountForTable(_selectedTableId);

        if (orderCount == 0)
        {
            if (!table.IsOccupied)
            {
                MessageBox.Show("Bu masa icin aktif siparis yok.", "Hesap Al", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var closeEmpty = MessageBox.Show(
                "Bu masada siparis yok. Masa kapatilsin mi?",
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
            $"{table.Name} hesap: {total:0.00} TL",
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

    private void AddTableButtonOnClick(object? sender, EventArgs e)
    {
        var createdTable = DatabaseHelper.AddTable();
        LoadTables();
        _selectedTableId = createdTable.Id;
        RefreshTableButtons();
        UpdateSelectedTableSection();
    }

    private void RemoveTableButtonOnClick(object? sender, EventArgs e)
    {
        if (_selectedTableId <= 0 || !_tableMap.ContainsKey(_selectedTableId))
        {
            MessageBox.Show("Once masa secin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var table = _tableMap[_selectedTableId];
        var orderCount = DatabaseHelper.GetOrderCountForTable(_selectedTableId);
        if (orderCount > 0 || table.IsOccupied)
        {
            var confirm = MessageBox.Show(
                "Bu masada aktif siparis var. Silmek isterseniz siparisler de silinir. Devam edilsin mi?",
                "Masa Sil",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }
        }

        DatabaseHelper.DeleteTable(_selectedTableId);
        _selectedTableId = 0;
        LoadTables();
        UpdateSelectedTableSection();
    }

    private void RenameTableButtonOnClick(object? sender, EventArgs e)
    {
        if (_selectedTableId <= 0 || !_tableMap.ContainsKey(_selectedTableId))
        {
            MessageBox.Show("Once masa secin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var table = _tableMap[_selectedTableId];
        var newName = ShowTextInputDialog("Masa Adi Duzenle", "Yeni masa adi:", table.Name);
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        DatabaseHelper.UpdateTableName(_selectedTableId, newName);
        LoadTables();
        UpdateSelectedTableSection();
    }

    private void UpdateSelectedTableSection()
    {
        if (_selectedTableId <= 0 || !_tableMap.ContainsKey(_selectedTableId))
        {
            _selectedTableLabel.Text = "Secili Masa: Yok";
            _ordersListBox.DataSource = null;
            _ordersListBox.Items.Clear();
            _totalPriceLabel.Text = "Toplam: 0.00 TL";
            return;
        }

        var table = _tableMap[_selectedTableId];
        var stateText = table.IsOccupied ? "Dolu" : "Musait";
        var minuteText = table.IsOccupied ? $", Gecen: {GetElapsedMinutes(table.StartTime)} dk" : string.Empty;
        _selectedTableLabel.Text = $"Secili Masa: {table.Name} ({stateText}{minuteText})";

        var orders = DatabaseHelper.GetOrdersForTable(_selectedTableId);
        _ordersListBox.DataSource = null;
        _ordersListBox.DataSource = orders;

        var total = DatabaseHelper.GetTotalForTable(_selectedTableId);
        _totalPriceLabel.Text = $"Toplam: {total:0.00} TL";
    }

    private static string? ShowTextInputDialog(string title, string prompt, string defaultValue)
    {
        using var dialog = new Form
        {
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            Width = 420,
            Height = 160,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var promptLabel = new Label
        {
            Text = prompt,
            AutoSize = true,
            Left = 12,
            Top = 12
        };

        var inputBox = new TextBox
        {
            Left = 12,
            Top = 36,
            Width = 380,
            Text = defaultValue
        };

        var okButton = new Button
        {
            Text = "Tamam",
            DialogResult = DialogResult.OK,
            Left = 232,
            Width = 75,
            Top = 72
        };

        var cancelButton = new Button
        {
            Text = "Iptal",
            DialogResult = DialogResult.Cancel,
            Left = 317,
            Width = 75,
            Top = 72
        };

        dialog.Controls.Add(promptLabel);
        dialog.Controls.Add(inputBox);
        dialog.Controls.Add(okButton);
        dialog.Controls.Add(cancelButton);
        dialog.AcceptButton = okButton;
        dialog.CancelButton = cancelButton;

        var result = dialog.ShowDialog();
        return result == DialogResult.OK ? inputBox.Text.Trim() : null;
    }

    private void UiTimerOnTick(object? sender, EventArgs e)
    {
        RefreshTableButtons();

        if (_selectedTableId > 0 && !_tableMap.ContainsKey(_selectedTableId))
        {
            _selectedTableId = 0;
        }

        if (_selectedTableId > 0)
        {
            UpdateSelectedTableSection();
        }
    }
}