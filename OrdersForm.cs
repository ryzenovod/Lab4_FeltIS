using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Lab4_FeltIS
{
    public partial class OrdersForm : Form
    {
        private ComboBox cbContract;
        private ComboBox cbModel;
        private NumericUpDown numQty;
        private NumericUpDown numAmount;
        private Button btnSave;

        public OrdersForm()
        {
            Text = "АРМ Менеджера заказов";
            Width = 600;
            Height = 350;

            Font = new Font("Segoe UI", 10);

            // ===== Подписи =====
            Controls.Add(new Label { Text = "Договор клиента:", Left = 20, Top = 20, AutoSize = true });
            Controls.Add(new Label { Text = "Модель валенок:", Left = 20, Top = 80, AutoSize = true });
            Controls.Add(new Label { Text = "Количество:", Left = 420, Top = 80, AutoSize = true });
            Controls.Add(new Label { Text = "Сумма оплаты (₽):", Left = 20, Top = 140, AutoSize = true });

            // ===== Поля =====
            cbContract = new ComboBox { Left = 20, Top = 45, Width = 500, DropDownStyle = ComboBoxStyle.DropDownList };
            cbModel = new ComboBox { Left = 20, Top = 105, Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
            numQty = new NumericUpDown { Left = 420, Top = 105, Width = 100, Minimum = 1, Maximum = 1000, Value = 1 };
            numAmount = new NumericUpDown { Left = 20, Top = 165, Width = 200, DecimalPlaces = 2, Maximum = 1000000 };

            btnSave = new Button
            {
                Text = "Сохранить заказ (транзакция)",
                Left = 20,
                Top = 220,
                Width = 300,
                Height = 40
            };

            btnSave.Click += BtnSave_Click;

            Controls.Add(cbContract);
            Controls.Add(cbModel);
            Controls.Add(numQty);
            Controls.Add(numAmount);
            Controls.Add(btnSave);

            LoadData();
        }

        private void LoadData()
        {
            // Договоры с именем клиента
            cbContract.DataSource = Db.Query(@"
SELECT cc.ContractID,
       CAST(cc.ContractID as nvarchar(10)) + ' — ' + c.Name AS DisplayText
FROM ClientContract cc
JOIN Client c ON c.ClientID = cc.ClientID
ORDER BY cc.ContractID");

            cbContract.DisplayMember = "DisplayText";
            cbContract.ValueMember = "ContractID";

            // Модели с размером
            cbModel.DataSource = Db.Query(@"
SELECT ModelID,
       Article + ' (Размер ' + CAST(Size as nvarchar(5)) + ')' AS DisplayText
FROM FeltModel
ORDER BY Article");

            cbModel.DisplayMember = "DisplayText";
            cbModel.ValueMember = "ModelID";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using (var cn = new SqlConnection(Db.ConnStr))
            {
                cn.Open();
                var tx = cn.BeginTransaction();

                try
                {
                    int contractId = Convert.ToInt32(cbContract.SelectedValue);
                    int modelId = Convert.ToInt32(cbModel.SelectedValue);
                    int qty = (int)numQty.Value;
                    decimal amount = numAmount.Value;

                    var cmd1 = new SqlCommand(
                        "INSERT INTO ClientOrder(ContractID) VALUES(@c); SELECT SCOPE_IDENTITY();",
                        cn, tx);
                    cmd1.Parameters.AddWithValue("@c", contractId);
                    int orderId = Convert.ToInt32(cmd1.ExecuteScalar());

                    var cmd2 = new SqlCommand(
                        "INSERT INTO OrderLine(OrderID, ModelID, Quantity) VALUES(@o,@m,@q)",
                        cn, tx);
                    cmd2.Parameters.AddWithValue("@o", orderId);
                    cmd2.Parameters.AddWithValue("@m", modelId);
                    cmd2.Parameters.AddWithValue("@q", qty);
                    cmd2.ExecuteNonQuery();

                    var cmd3 = new SqlCommand(
                        "INSERT INTO Payment(OrderID, Amount) VALUES(@o,@a)",
                        cn, tx);
                    cmd3.Parameters.AddWithValue("@o", orderId);
                    cmd3.Parameters.AddWithValue("@a", amount);
                    cmd3.ExecuteNonQuery();

                    tx.Commit();

                    MessageBox.Show("Заказ успешно сохранён.", "Готово");

                    new OrderDocumentForm(orderId).ShowDialog();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }
    }
}