using System;
using System.Windows.Forms;

namespace Lab4_FeltIS
{
    public partial class MainForm : Form
    {
        private Button btnTestDb;

        public MainForm()
        {
            Text = "Lab4 FeltIS";
            Width = 900;
            Height = 500;

            // ===== Кнопка Тест БД =====
            btnTestDb = new Button();
            btnTestDb.Text = "Тест БД";
            btnTestDb.Left = 20;
            btnTestDb.Top = 20;
            btnTestDb.Width = 150;
            btnTestDb.Height = 35;
            btnTestDb.Click += BtnTestDb_Click;
            Controls.Add(btnTestDb);

            // ===== Кнопка АРМ Заказы =====
            var btnOrders = new Button();
            btnOrders.Text = "АРМ Заказы";
            btnOrders.Left = 20;
            btnOrders.Top = 80;
            btnOrders.Width = 150;
            btnOrders.Height = 35;
            btnOrders.Click += (s, e) => new OrdersForm().ShowDialog();
            Controls.Add(btnOrders);

            // ===== Кнопка Журнал =====
            var btnJournal = new Button();
            btnJournal.Text = "Журнал (сегодня)";
            btnJournal.Left = 20;
            btnJournal.Top = 140;
            btnJournal.Width = 150;
            btnJournal.Height = 35;
            btnJournal.Click += (s, e) => new JournalForm().ShowDialog();
            Controls.Add(btnJournal);

            // ===== Кнопка Отчет =====
            var btnReport = new Button();
            btnReport.Text = "Отчет";
            btnReport.Left = 20;
            btnReport.Top = 200;
            btnReport.Width = 150;
            btnReport.Height = 35;
            btnReport.Click += (s, e) => new ReportForm().ShowDialog();
            Controls.Add(btnReport);
        }

        private void BtnTestDb_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = Db.Query("SELECT TOP 1 ClientID, Name FROM dbo.Client ORDER BY ClientID;");
                MessageBox.Show("OK. Клиент: " + dt.Rows[0]["Name"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}