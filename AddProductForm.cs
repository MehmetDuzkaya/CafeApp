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

    public string ProductCategory { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public decimal ProductPrice { get; private set; }

    public AddProductForm(List<string> existingCategories)
    {
        ConfigureForm();
        BuildLayout();
        LoadCategories(existingCategories);
    }

    private void ConfigureForm()
    {
        Text = "Add Product";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 420;
        Height = 280;
    }

    private void BuildLayout()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(14)
        };

        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95f));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var categoryLabel = new Label
        {
            Text = "Category:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };

        _categoryComboBox.Dock = DockStyle.Top;
        _categoryComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        _categoryComboBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var nameLabel = new Label
        {
            Text = "Name:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };

        _nameTextBox.Dock = DockStyle.Top;
        _nameTextBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var priceLabel = new Label
        {
            Text = "Price:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };

        _priceTextBox.Dock = DockStyle.Top;
        _priceTextBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0)
        };

        var saveButton = new Button
        {
            Text = "Save",
            AutoSize = true,
            BackColor = Color.FromArgb(32, 153, 95),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(10, 0, 0, 0)
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += SaveButtonOnClick;

        var cancelButton = new Button
        {
            Text = "Cancel",
            AutoSize = true,
            FlatStyle = FlatStyle.Standard,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };
        cancelButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        buttonsPanel.Controls.Add(saveButton);
        buttonsPanel.Controls.Add(cancelButton);

        mainLayout.Controls.Add(categoryLabel, 0, 0);
        mainLayout.Controls.Add(_categoryComboBox, 1, 0);
        mainLayout.Controls.Add(nameLabel, 0, 1);
        mainLayout.Controls.Add(_nameTextBox, 1, 1);
        mainLayout.Controls.Add(priceLabel, 0, 2);
        mainLayout.Controls.Add(_priceTextBox, 1, 2);
        mainLayout.Controls.Add(buttonsPanel, 1, 3);

        Controls.Add(mainLayout);

        AcceptButton = saveButton;
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

    private void SaveButtonOnClick(object? sender, EventArgs e)
    {
        var category = _categoryComboBox.Text.Trim();
        var name = _nameTextBox.Text.Trim();
        var priceText = _priceTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(category))
        {
            MessageBox.Show("Please enter a category.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

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

        ProductCategory = category;
        ProductName = name;
        ProductPrice = price;

        DialogResult = DialogResult.OK;
        Close();
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