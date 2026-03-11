using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Lab4_FeltIS
{
    public partial class JournalForm : Form
    {
        private DateTimePicker dtFrom;
        private DateTimePicker dtTo;
        private NumericUpDown numMinAmount;
        private TextBox txtOrderId;
        private DataGridView grid;
        private Label lblSummary;

        public JournalForm()
        {
            Text = "Журнал платежей";
            Width = 920;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);

            var topPanel = new Panel { Dock = DockStyle.Top, Height = 92, Padding = new Padding(12), BackColor = Color.FromArgb(247, 249, 252) };
            Controls.Add(topPanel);

            topPanel.Controls.Add(new Label { Text = "Период с", Left = 10, Top = 12, AutoSize = true });
            dtFrom = new DateTimePicker { Left = 10, Top = 34, Width = 150, Value = DateTime.Today };
            topPanel.Controls.Add(dtFrom);

            topPanel.Controls.Add(new Label { Text = "по", Left = 175, Top = 12, AutoSize = true });
            dtTo = new DateTimePicker { Left = 175, Top = 34, Width = 150, Value = DateTime.Today };
            topPanel.Controls.Add(dtTo);

            topPanel.Controls.Add(new Label { Text = "Мин. сумма", Left = 340, Top = 12, AutoSize = true });
            numMinAmount = new NumericUpDown { Left = 340, Top = 34, Width = 120, DecimalPlaces = 2, Maximum = 1000000 };
            topPanel.Controls.Add(numMinAmount);

            topPanel.Controls.Add(new Label { Text = "№ заказа", Left = 475, Top = 12, AutoSize = true });
            txtOrderId = new TextBox { Left = 475, Top = 34, Width = 110 };
            topPanel.Controls.Add(txtOrderId);

            var btnApply = new Button { Text = "Применить", Left = 600, Top = 31, Width = 120, Height = 32, BackColor = Color.FromArgb(65, 133, 244), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += (s, e) => LoadJournal();
            topPanel.Controls.Add(btnApply);

            var btnExport = new Button { Text = "Экспорт CSV", Left = 730, Top = 31, Width = 140, Height = 32, BackColor = Color.FromArgb(15, 157, 88), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            topPanel.Controls.Add(btnExport);

            lblSummary = new Label { Left = 10, Top = 67, Width = 840, Height = 20, ForeColor = Color.FromArgb(80, 80, 80) };
            topPanel.Controls.Add(lblSummary);

            grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            Controls.Add(grid);

            LoadJournal();
        }

        private void LoadJournal()
        {
            var from = dtFrom.Value.Date;
            var to = dtTo.Value.Date.AddDays(1).AddTicks(-1);
            var minAmount = numMinAmount.Value;

            int orderId;
            bool hasOrderFilter = int.TryParse(txtOrderId.Text.Trim(), out orderId);

            var sql = @"
SELECT PaymentID, OrderID, Amount, PaymentDate
FROM Payment
WHERE PaymentDate >= '" + from.ToString("yyyy-MM-dd") + @"'
  AND PaymentDate <= '" + to.ToString("yyyy-MM-dd HH:mm:ss") + @"'
  AND Amount >= " + minAmount.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (hasOrderFilter)
            {
                sql += " AND OrderID = " + orderId;
            }

            sql += " ORDER BY PaymentDate DESC";

            var dt = Db.Query(sql);
            grid.DataSource = dt;

            decimal totalAmount = 0;
            foreach (DataRow row in dt.Rows)
            {
                totalAmount += Convert.ToDecimal(row["Amount"]);
            }

            lblSummary.Text = "Записей: " + dt.Rows.Count + " · Общая сумма: " + totalAmount.ToString("N2") + " ₽";
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (!(grid.DataSource is DataTable dt) || dt.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = "payments_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                var sb = new StringBuilder();

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sb.Append(dt.Columns[i].ColumnName);
                    if (i < dt.Columns.Count - 1) sb.Append(';');
                }
                sb.AppendLine();

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        sb.Append(row[i].ToString());
                        if (i < dt.Columns.Count - 1) sb.Append(';');
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Экспорт завершён: " + sfd.FileName, "Готово");
            }
        }
    }
}
