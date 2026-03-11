using System;
using System.Windows.Forms;

namespace Lab4_FeltIS
{
    public partial class JournalForm : Form
    {
        public JournalForm()
        {
            Text = "Журнал (сегодня)";
            Width = 600;
            Height = 400;

            var grid = new DataGridView { Dock = DockStyle.Fill };
            Controls.Add(grid);

            grid.DataSource = Db.Query(@"
SELECT PaymentID, OrderID, Amount, PaymentDate
FROM Payment
WHERE PaymentDate >= CONVERT(date, GETDATE())
");
        }
    }
}