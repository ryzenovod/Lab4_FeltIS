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

            Text = "FeltIS · Панель управления";
            Width = 980;
            Height = 620;
            MinimumSize = new Size(900, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 247, 251);
            Font = new Font("Segoe UI", 10F);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.FromArgb(32, 57, 99)
            };

            var title = new Label
            {
                Text = "FeltIS",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold),
                AutoSize = true,
                Left = 24,
                Top = 18
            };

            var subtitle = new Label
            {
                Text = "Управление заказами, платежами и аналитикой в одном окне",
                ForeColor = Color.FromArgb(215, 225, 245),
                AutoSize = true,
                Left = 27,
                Top = 68
            };

            lblDbStatus = new Label
            {
                Text = "Проверка подключения к базе...",
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Width = 380,
                Height = 24,
                Top = 42,
                Left = Width - 430,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(lblDbStatus);
            Controls.Add(header);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                ColumnCount = 2,
                RowCount = 2
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            content.Controls.Add(CreateFeatureCard("Новый заказ", "Оформление заказа с транзакционным сохранением.", "Открыть", () => new OrdersForm().ShowDialog(), Color.FromArgb(65, 133, 244)), 0, 0);
            content.Controls.Add(CreateFeatureCard("Журнал платежей", "Фильтрация по периоду, поиску и сумме.", "Открыть", () => new JournalForm().ShowDialog(), Color.FromArgb(15, 157, 88)), 1, 0);
            content.Controls.Add(CreateFeatureCard("Отчёт по моделям", "Аналитика продаж с выбором периода.", "Открыть", () => new ReportForm().ShowDialog(), Color.FromArgb(244, 160, 0)), 0, 1);
            content.Controls.Add(CreateFeatureCard("Проверка БД", "Быстрый тест подключения и чтения данных.", "Проверить", () => CheckDbConnection(true), Color.FromArgb(171, 71, 188)), 1, 1);

            Controls.Add(content);
        }

        private Panel CreateFeatureCard(string title, string description, string buttonText, Action action, Color accent)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                BackColor = Color.White,
                Padding = new Padding(18)
            };

            var accentBar = new Panel
            {
                BackColor = accent,
                Dock = DockStyle.Top,
                Height = 6
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                AutoSize = true,
                Top = 26,
                Left = 18
            };

            var lblDesc = new Label
            {
                Text = description,
                ForeColor = Color.FromArgb(70, 70, 70),
                AutoSize = false,
                Width = 360,
                Height = 56,
                Top = 64,
                Left = 18
            };

            var button = new Button
            {
                Text = buttonText,
                Width = 140,
                Height = 36,
                Left = 18,
                Top = 130,
                BackColor = accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (s, e) => action();

            card.Controls.Add(accentBar);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblDesc);
            card.Controls.Add(button);

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
