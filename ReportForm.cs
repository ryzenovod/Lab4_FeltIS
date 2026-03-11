using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Lab4_FeltIS
{
    public partial class ReportForm : Form
    {
        private DateTimePicker dtFrom;
        private DateTimePicker dtTo;
        private DataGridView grid;
        private Label lblBestModel;

        public ReportForm()
        {
            Text = "Отчёт по моделям";
            Width = 840;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);

            var top = new Panel { Dock = DockStyle.Top, Height = 90, Padding = new Padding(12), BackColor = Color.FromArgb(247, 249, 252) };
            Controls.Add(top);

            top.Controls.Add(new Label { Text = "Период с", Left = 10, Top = 12, AutoSize = true });
            dtFrom = new DateTimePicker { Left = 10, Top = 34, Width = 170, Value = DateTime.Today.AddMonths(-1) };
            top.Controls.Add(dtFrom);

            top.Controls.Add(new Label { Text = "по", Left = 190, Top = 12, AutoSize = true });
            dtTo = new DateTimePicker { Left = 190, Top = 34, Width = 170, Value = DateTime.Today };
            top.Controls.Add(dtTo);

            var btnRefresh = new Button { Text = "Обновить", Left = 380, Top = 31, Width = 120, Height = 32, BackColor = Color.FromArgb(65, 133, 244), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadReport();
            top.Controls.Add(btnRefresh);

            lblBestModel = new Label { Left = 10, Top = 67, Width = 760, Height = 20, ForeColor = Color.FromArgb(80, 80, 80) };
            top.Controls.Add(lblBestModel);

            grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            Controls.Add(grid);

            LoadReport();
        }

        private void LoadReport()
        {
            var from = dtFrom.Value.Date;
            var to = dtTo.Value.Date.AddDays(1).AddTicks(-1);

            var dt = Db.Query(@"
SELECT fm.Article,
       SUM(ol.Quantity) AS TotalQty,
       SUM(p.Amount) AS TotalAmount
FROM Payment p
JOIN ClientOrder o ON o.OrderID = p.OrderID
JOIN OrderLine ol ON ol.OrderID = o.OrderID
JOIN FeltModel fm ON fm.ModelID = ol.ModelID
WHERE p.PaymentDate >= '" + from.ToString("yyyy-MM-dd") + @"'
  AND p.PaymentDate <= '" + to.ToString("yyyy-MM-dd HH:mm:ss") + @"'
GROUP BY fm.Article
ORDER BY TotalQty DESC");

            grid.DataSource = dt;

            if (dt.Rows.Count > 0)
            {
                lblBestModel.Text = "Лидер периода: " + dt.Rows[0]["Article"] +
                                    " · Кол-во: " + dt.Rows[0]["TotalQty"] +
                                    " · Выручка: " + Convert.ToDecimal(dt.Rows[0]["TotalAmount"]).ToString("N2") + " ₽";
            }
            else
            {
                lblBestModel.Text = "За выбранный период продаж нет.";
            }
        }
    }
}
