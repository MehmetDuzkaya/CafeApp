using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CafeApp;

public sealed class AddProductForm : Form
{
    private readonly ComboBox _categoryComboBox = new();
    private readonly TextBox _nameTextBox = new();
    private readonly TextBox _priceTextBox = new();
    private readonly ListBox _productsListBox = new();

    private bool _hasChanges;

    public bool HasChanges => _hasChanges;

    public AddProductForm(List<string> existingCategories)
    {
        ConfigureForm();
        BuildLayout();
        LoadCategories(existingCategories);
        LoadProducts();
    }

    private void ConfigureForm()
    {
        Text = "Product Management";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 760;
        Height = 480;
    }

    private void BuildLayout()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(14)
        };

        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47f));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53f));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, 8, 0)
        };
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var listTitle = new Label
        {
            Text = "Product List (select to edit/delete)",
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(0, 0, 0, 6)
        };

        _productsListBox.Dock = DockStyle.Fill;
        _productsListBox.Font = new Font("Segoe UI", 9, FontStyle.Regular);
        _productsListBox.SelectedIndexChanged += ProductsListBoxOnSelectedIndexChanged;

        leftPanel.Controls.Add(listTitle, 0, 0);
        leftPanel.Controls.Add(_productsListBox, 0, 1);

        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Margin = new Padding(8, 0, 0, 0)
        };
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95f));
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var formTitle = new Label
        {
            Text = "Add / Update Product",
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(0, 0, 0, 6)
        };
        rightPanel.Controls.Add(formTitle, 1, 0);

        var categoryLabel = new Label
        {
            Text = "Category:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };

        _categoryComboBox.Dock = DockStyle.Fill;
        _categoryComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        _categoryComboBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var nameLabel = new Label
        {
            Text = "Name:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };

        _nameTextBox.Dock = DockStyle.Fill;
        _nameTextBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var priceLabel = new Label
        {
            Text = "Price:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };

        _priceTextBox.Dock = DockStyle.Fill;
        _priceTextBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 12, 0, 0)
        };

        var addButton = new Button
        {
            Text = "Yeni Ekle",
            AutoSize = true,
            BackColor = Color.FromArgb(32, 153, 95),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(0, 0, 8, 0)
        };
        addButton.FlatAppearance.BorderSize = 0;
        addButton.Click += AddButtonOnClick;

        var updateButton = new Button
        {
            Text = "Guncelle",
            AutoSize = true,
            BackColor = Color.FromArgb(60, 120, 200),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(0, 0, 8, 0)
        };
        updateButton.FlatAppearance.BorderSize = 0;
        updateButton.Click += UpdateButtonOnClick;

        var deleteButton = new Button
        {
            Text = "Sil",
            AutoSize = true,
            BackColor = Color.FromArgb(193, 62, 62),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(0, 0, 8, 0)
        };
        deleteButton.FlatAppearance.BorderSize = 0;
        deleteButton.Click += DeleteButtonOnClick;

        var cancelButton = new Button
        {
            Text = "Kapat",
            AutoSize = true,
            FlatStyle = FlatStyle.Standard,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };
        cancelButton.Click += (_, _) =>
        {
            DialogResult = _hasChanges ? DialogResult.OK : DialogResult.Cancel;
            Close();
        };

        buttonsPanel.Controls.Add(addButton);
        buttonsPanel.Controls.Add(updateButton);
        buttonsPanel.Controls.Add(deleteButton);
        buttonsPanel.Controls.Add(cancelButton);

        rightPanel.Controls.Add(categoryLabel, 0, 1);
        rightPanel.Controls.Add(_categoryComboBox, 1, 1);
        rightPanel.Controls.Add(nameLabel, 0, 2);
        rightPanel.Controls.Add(_nameTextBox, 1, 2);
        rightPanel.Controls.Add(priceLabel, 0, 3);
        rightPanel.Controls.Add(_priceTextBox, 1, 3);
        rightPanel.Controls.Add(buttonsPanel, 1, 4);

        mainLayout.Controls.Add(leftPanel, 0, 0);
        mainLayout.Controls.Add(rightPanel, 1, 0);

        Controls.Add(mainLayout);

        AcceptButton = addButton;
        CancelButton = cancelButton;
    }

    private void LoadCategories(List<string> existingCategories)
    {
        var defaults = new[]
        {
            "Kahve",
            "Cay",
            "Soguk Icecek",
            "Tatli",
            "Yiyecek",
            "Genel"
        };

        foreach (var category in defaults)
        {
            AddCategoryIfMissing(category);
        }

        foreach (var category in existingCategories)
        {
            AddCategoryIfMissing(category);
        }

        if (_categoryComboBox.Items.Count > 0)
        {
            _categoryComboBox.SelectedIndex = 0;
        }
    }

    private void LoadProducts()
    {
        var products = DatabaseHelper.GetProducts();
        _productsListBox.DataSource = null;
        _productsListBox.DataSource = products;
    }

    private void AddCategoryIfMissing(string category)
    {
        var normalized = category.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        foreach (var item in _categoryComboBox.Items)
        {
            if (string.Equals(Convert.ToString(item), normalized, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        _categoryComboBox.Items.Add(normalized);
    }

    private void ProductsListBoxOnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_productsListBox.SelectedItem is not ProductInfo selectedProduct)
        {
            return;
        }

        AddCategoryIfMissing(selectedProduct.Category);
        _categoryComboBox.Text = selectedProduct.Category;
        _nameTextBox.Text = selectedProduct.Name;
        _priceTextBox.Text = selectedProduct.Price.ToString("0.00", CultureInfo.CurrentCulture);
    }

    private bool TryReadInputs(out string category, out string name, out decimal price)
    {
        category = _categoryComboBox.Text.Trim();
        name = _nameTextBox.Text.Trim();
        var priceText = _priceTextBox.Text.Trim();
        price = 0m;

        if (string.IsNullOrWhiteSpace(category))
        {
            MessageBox.Show("Please enter a category.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a product name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!TryParsePrice(priceText, out var price) || price <= 0)
        {
            MessageBox.Show("Please enter a valid price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        return true;
    }

    private void AddButtonOnClick(object? sender, EventArgs e)
    {
        if (!TryReadInputs(out var category, out var name, out var price))
        {
            return;
        }

        DatabaseHelper.AddProduct(category, name, price);
        _hasChanges = true;
        AddCategoryIfMissing(category);
        LoadProducts();
        _nameTextBox.Clear();
        _priceTextBox.Clear();
    }

    private void UpdateButtonOnClick(object? sender, EventArgs e)
    {
        if (_productsListBox.SelectedItem is not ProductInfo selectedProduct)
        {
            MessageBox.Show("Please select a product from the list.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!TryReadInputs(out var category, out var name, out var price))
        {
            return;
        }

        DatabaseHelper.UpdateProduct(selectedProduct.Id, category, name, price);
        _hasChanges = true;
        AddCategoryIfMissing(category);
        LoadProducts();
    }

    private void DeleteButtonOnClick(object? sender, EventArgs e)
    {
        if (_productsListBox.SelectedItem is not ProductInfo selectedProduct)
        {
            MessageBox.Show("Please select a product from the list.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Delete product '{selectedProduct.Name}'?",
            "Confirm",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {
            return;
        }

        DatabaseHelper.DeleteProduct(selectedProduct.Id);
        _hasChanges = true;
        LoadProducts();

        if (_productsListBox.Items.Count == 0)
        {
            _nameTextBox.Clear();
            _priceTextBox.Clear();
        }
    }

    private static bool TryParsePrice(string value, out decimal price)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out price))
        {
            return true;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out price);
    }
}