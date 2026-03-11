using System.Windows.Forms;

namespace Lab4_FeltIS
{
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            Text = "Отчет по моделям";
            Width = 700;
            Height = 400;

            var grid = new DataGridView { Dock = DockStyle.Fill };
            Controls.Add(grid);

            grid.DataSource = Db.Query(@"
SELECT fm.Article,
       SUM(ol.Quantity) AS TotalQty,
       SUM(p.Amount) AS TotalAmount
FROM Payment p
JOIN ClientOrder o ON o.OrderID = p.OrderID
JOIN OrderLine ol ON ol.OrderID = o.OrderID
JOIN FeltModel fm ON fm.ModelID = ol.ModelID
GROUP BY fm.Article
ORDER BY TotalQty DESC");
        }
    }
}