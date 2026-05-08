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
    private readonly Button _addCategoryButton = new();
    private readonly Button _removeCategoryButton = new();

    private bool _hasChanges;

    public bool HasChanges => _hasChanges;

    public AddProductForm()
    {
        ConfigureForm();
        BuildLayout();
        LoadCategories();
        LoadProducts();
    }

    private void ConfigureForm()
    {
        Text = "Urun Yonetimi";
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
            Text = "Urun Listesi (duzenle/sil icin secin)",
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
            RowCount = 6,
            Margin = new Padding(8, 0, 0, 0)
        };
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95f));
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var formTitle = new Label
        {
            Text = "Urun Ekle / Guncelle",
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(0, 0, 0, 6)
        };
        rightPanel.Controls.Add(formTitle, 1, 0);

        var categoryLabel = new Label
        {
            Text = "Kategori:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };

        _categoryComboBox.Dock = DockStyle.Fill;
        _categoryComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        _categoryComboBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var nameLabel = new Label
        {
            Text = "Ad:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };

        _nameTextBox.Dock = DockStyle.Fill;
        _nameTextBox.Font = new Font("Segoe UI", 10, FontStyle.Regular);

        var priceLabel = new Label
        {
            Text = "Fiyat:",
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
        var categoryActionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 4, 0, 0)
        };

        _addCategoryButton.Text = "Kategori Ekle";
        _addCategoryButton.AutoSize = true;
        _addCategoryButton.BackColor = Color.FromArgb(32, 153, 95);
        _addCategoryButton.ForeColor = Color.White;
        _addCategoryButton.FlatStyle = FlatStyle.Flat;
        _addCategoryButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _addCategoryButton.Margin = new Padding(0, 0, 8, 0);
        _addCategoryButton.FlatAppearance.BorderSize = 0;
        _addCategoryButton.Click += AddCategoryButtonOnClick;

        _removeCategoryButton.Text = "Kategori Sil";
        _removeCategoryButton.AutoSize = true;
        _removeCategoryButton.BackColor = Color.FromArgb(193, 62, 62);
        _removeCategoryButton.ForeColor = Color.White;
        _removeCategoryButton.FlatStyle = FlatStyle.Flat;
        _removeCategoryButton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _removeCategoryButton.FlatAppearance.BorderSize = 0;
        _removeCategoryButton.Click += RemoveCategoryButtonOnClick;

        categoryActionsPanel.Controls.Add(_addCategoryButton);
        categoryActionsPanel.Controls.Add(_removeCategoryButton);

        rightPanel.Controls.Add(nameLabel, 0, 3);
        rightPanel.Controls.Add(_nameTextBox, 1, 3);
        rightPanel.Controls.Add(priceLabel, 0, 4);
        rightPanel.Controls.Add(_priceTextBox, 1, 4);
        rightPanel.Controls.Add(buttonsPanel, 1, 5);
        rightPanel.Controls.Add(categoryActionsPanel, 1, 2);

        mainLayout.Controls.Add(leftPanel, 0, 0);
        mainLayout.Controls.Add(rightPanel, 1, 0);

        Controls.Add(mainLayout);

        AcceptButton = addButton;
        CancelButton = cancelButton;
    }

    private void LoadCategories()
    {
        _categoryComboBox.Items.Clear();

        var categories = DatabaseHelper.GetCategories();
        foreach (var category in categories)
        {
            AddCategoryIfMissing(category);
        }

        if (_categoryComboBox.Items.Count == 0)
        {
            AddCategoryIfMissing("Genel");
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

    private void ReloadCategories(string? preferredSelection = null)
    {
        var selection = preferredSelection ?? _categoryComboBox.Text;
        _categoryComboBox.Items.Clear();

        var categories = DatabaseHelper.GetCategories();
        foreach (var category in categories)
        {
            AddCategoryIfMissing(category);
        }

        if (_categoryComboBox.Items.Count == 0)
        {
            AddCategoryIfMissing("Genel");
        }

        if (!string.IsNullOrWhiteSpace(selection))
        {
            _categoryComboBox.Text = selection;
        }
        else if (_categoryComboBox.Items.Count > 0)
        {
            _categoryComboBox.SelectedIndex = 0;
        }
    }

    private void AddCategoryButtonOnClick(object? sender, EventArgs e)
    {
        var categoryName = ShowTextInputDialog("Kategori Ekle", "Yeni kategori adi:", string.Empty);
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return;
        }

        DatabaseHelper.AddCategory(categoryName);
        ReloadCategories(categoryName);
        _hasChanges = true;
    }

    private void RemoveCategoryButtonOnClick(object? sender, EventArgs e)
    {
        var selectedCategory = _categoryComboBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(selectedCategory))
        {
            MessageBox.Show("Silinecek kategori secin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.Equals(selectedCategory, "Genel", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("'Genel' kategorisi silinemez.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var productCount = DatabaseHelper.GetProductCountForCategory(selectedCategory);
        if (productCount > 0)
        {
            var reassign = MessageBox.Show(
                $"Bu kategoriye ait {productCount} urun var. Bu urunler 'Genel' kategorisine tasinsin mi?",
                "Kategori Sil",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (reassign != DialogResult.Yes)
            {
                return;
            }

            DatabaseHelper.ReassignProductsToCategory(selectedCategory, "Genel");
        }

        DatabaseHelper.DeleteCategory(selectedCategory);
        ReloadCategories("Genel");
        LoadProducts();
        _hasChanges = true;
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
            MessageBox.Show("Kategori girin.", "Dogrulama", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Urun adi girin.", "Dogrulama", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!TryParsePrice(priceText, out var parsedPrice) || parsedPrice <= 0)
        {
            MessageBox.Show("Gecerli bir fiyat girin.", "Dogrulama", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        price = parsedPrice;

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
            MessageBox.Show("Listeden bir urun secin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            MessageBox.Show("Listeden bir urun secin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Urun silinsin mi: '{selectedProduct.Name}'?",
            "Onay",
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
}