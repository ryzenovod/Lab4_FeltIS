using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lab4_FeltIS
{
    public partial class MainForm : Form
    {
        private Label lblDbStatus;

        public MainForm()
        {
            InitializeComponent();

            Text = "Лабораторная работа №4";
            Width = 1040;
            Height = 680;
            MinimumSize = new Size(920, 580);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 247, 251);
            Font = new Font("Segoe UI", 10F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var header = BuildHeader();
            root.Controls.Add(header, 0, 0);

            var content = BuildContent();
            root.Controls.Add(content, 0, 1);
        }

        private Panel BuildHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(32, 57, 99),
                Padding = new Padding(24, 18, 24, 18)
            };

            var title = new Label
            {
                Text = "Лабораторная работа №4",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold),
                AutoSize = true,
                Dock = DockStyle.Top
            };

            var subtitle = new Label
            {
                Text = "Управление заказами, платежами и отчётностью",
                ForeColor = Color.FromArgb(215, 225, 245),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 8, 0, 0)
            };

            lblDbStatus = new Label
            {
                Text = "Проверка подключения к базе...",
                ForeColor = Color.White,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.TopRight,
                Location = new Point(730, 24)
            };

            header.Controls.Add(lblDbStatus);
            header.Controls.Add(subtitle);
            header.Controls.Add(title);

            header.Resize += (s, e) =>
            {
                lblDbStatus.Left = Math.Max(24, header.Width - lblDbStatus.Width - 24);
                lblDbStatus.Top = 26;
            };

            return header;
        }

        private TableLayoutPanel BuildContent()
        {
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 2
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            content.Controls.Add(CreateFeatureCard(
                "Новый заказ",
                "Оформление заказа с транзакционным сохранением.",
                "Открыть",
                () => new OrdersForm().ShowDialog(),
                Color.FromArgb(65, 133, 244)), 0, 0);

            content.Controls.Add(CreateFeatureCard(
                "Журнал платежей",
                "Фильтрация по периоду, поиску и сумме.",
                "Открыть",
                () => new JournalForm().ShowDialog(),
                Color.FromArgb(15, 157, 88)), 1, 0);

            content.Controls.Add(CreateFeatureCard(
                "Отчёт по моделям",
                "Аналитика продаж с выбором периода.",
                "Открыть",
                () => new ReportForm().ShowDialog(),
                Color.FromArgb(244, 160, 0)), 0, 1);

            content.Controls.Add(CreateFeatureCard(
                "Проверка БД",
                "Быстрый тест подключения и чтения данных.",
                "Проверить",
                () => CheckDbConnection(true),
                Color.FromArgb(171, 71, 188)), 1, 1);

            return content;
        }

        private Panel CreateFeatureCard(string title, string description, string buttonText, Action action, Color accent)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                BackColor = Color.White,
                Padding = new Padding(14)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 6));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

            var accentBar = new Panel
            {
                BackColor = accent,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                AutoEllipsis = true,
                Margin = new Padding(2, 0, 2, 0)
            };

            var lblDesc = new Label
            {
                Text = description,
                ForeColor = Color.FromArgb(70, 70, 70),
                Dock = DockStyle.Fill,
                AutoSize = false,
                Margin = new Padding(2, 6, 2, 6)
            };

            var button = new Button
            {
                Text = buttonText,
                Width = 150,
                Height = 36,
                BackColor = accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (s, e) => action();

            var buttonPanel = new Panel { Dock = DockStyle.Fill };
            button.Location = new Point(2, 8);
            buttonPanel.Controls.Add(button);

            layout.Controls.Add(accentBar, 0, 0);
            layout.Controls.Add(lblTitle, 0, 1);
            layout.Controls.Add(lblDesc, 0, 2);
            layout.Controls.Add(buttonPanel, 0, 3);

            card.Controls.Add(layout);
            return card;
        }

        private void BtnTestDb_Click(object sender, EventArgs e)
        {
            CheckDbConnection(true);
        }

        private void CheckDbConnection(bool showDialog)
        {
            try
            {
                var dt = Db.Query("SELECT TOP 1 ClientID, Name FROM dbo.Client ORDER BY ClientID;");
                lblDbStatus.Text = "База данных: подключение активно";
                lblDbStatus.ForeColor = Color.FromArgb(162, 245, 168);

                if (showDialog)
                {
                    MessageBox.Show("OK. Клиент: " + dt.Rows[0]["Name"], "Проверка БД");
                }
            }
            catch (Exception ex)
            {
                lblDbStatus.Text = "База данных: ошибка подключения";
                lblDbStatus.ForeColor = Color.FromArgb(255, 176, 176);

                if (showDialog)
                {
                    MessageBox.Show("Ошибка: " + ex.Message, "Проверка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            CheckDbConnection(false);
        }
    }
}
