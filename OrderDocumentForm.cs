using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Lab4_FeltIS
{
    public partial class OrderDocumentForm : Form
    {
        public OrderDocumentForm(int orderId)
        {
            Text = "Документ заказа";
            Width = 450;
            Height = 300;

            Font = new Font("Segoe UI", 10);

            var box = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true };
            Controls.Add(box);

            var dt = Db.Query($@"
SELECT o.OrderID, p.Amount, p.PaymentDate
FROM ClientOrder o
JOIN Payment p ON p.OrderID = o.OrderID
WHERE o.OrderID = {orderId}");

            if (dt.Rows.Count > 0)
            {
                box.Text =
                    "ДОКУМЕНТ ПЕРВИЧНОГО УЧЁТА\n\n" +
                    "Номер заказа: " + dt.Rows[0]["OrderID"] + "\n" +
                    "Сумма оплаты: " + dt.Rows[0]["Amount"] + " ₽\n" +
                    "Дата оплаты: " + dt.Rows[0]["PaymentDate"];
            }
        }
    }
}