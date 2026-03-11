using System;
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
        private Label lblHint;

        public OrdersForm()
        {
            Text = "АРМ Менеджера заказов";
            Width = 720;
            Height = 430;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(247, 249, 252);
            Font = new Font("Segoe UI", 10);

            var panel = new Panel
            {
                Left = 18,
                Top = 18,
                Width = 660,
                Height = 360,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            panel.Controls.Add(new Label { Text = "Договор клиента:", Left = 20, Top = 24, AutoSize = true });
            panel.Controls.Add(new Label { Text = "Модель валенок:", Left = 20, Top = 92, AutoSize = true });
            panel.Controls.Add(new Label { Text = "Количество:", Left = 480, Top = 92, AutoSize = true });
            panel.Controls.Add(new Label { Text = "Сумма оплаты (₽):", Left = 20, Top = 160, AutoSize = true });

            cbContract = new ComboBox { Left = 20, Top = 49, Width = 600, DropDownStyle = ComboBoxStyle.DropDownList };
            cbModel = new ComboBox { Left = 20, Top = 117, Width = 440, DropDownStyle = ComboBoxStyle.DropDownList };
            numQty = new NumericUpDown { Left = 480, Top = 117, Width = 140, Minimum = 1, Maximum = 1000, Value = 1 };
            numAmount = new NumericUpDown { Left = 20, Top = 185, Width = 210, DecimalPlaces = 2, Maximum = 1000000 };

            var btnSuggestAmount = new Button
            {
                Text = "Подсказать сумму",
                Left = 250,
                Top = 184,
                Width = 170,
                Height = 34,
                BackColor = Color.FromArgb(65, 133, 244),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSuggestAmount.FlatAppearance.BorderSize = 0;
            btnSuggestAmount.Click += BtnSuggestAmount_Click;

            var btnSave = new Button
            {
                Text = "Сохранить заказ",
                Left = 20,
                Top = 255,
                Width = 220,
                Height = 42,
                BackColor = Color.FromArgb(15, 157, 88),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;

            lblHint = new Label
            {
                Left = 20,
                Top = 228,
                Width = 600,
                Height = 24,
                ForeColor = Color.FromArgb(80, 80, 80),
                Text = "Подсказка: выберите модель и нажмите «Подсказать сумму»"
            };

            btnSave.Click += BtnSave_Click;

            panel.Controls.Add(cbContract);
            panel.Controls.Add(cbModel);
            panel.Controls.Add(numQty);
            panel.Controls.Add(numAmount);
            panel.Controls.Add(btnSuggestAmount);
            panel.Controls.Add(lblHint);
            panel.Controls.Add(btnSave);
            Controls.Add(panel);

            LoadData();
        }

        private void LoadData()
        {
            cbContract.DataSource = Db.Query(@"
SELECT cc.ContractID,
       CAST(cc.ContractID as nvarchar(10)) + ' — ' + c.Name AS DisplayText
FROM ClientContract cc
JOIN Client c ON c.ClientID = cc.ClientID
ORDER BY cc.ContractID");

            cbContract.DisplayMember = "DisplayText";
            cbContract.ValueMember = "ContractID";

            cbModel.DataSource = Db.Query(@"
SELECT ModelID,
       Article + ' (Размер ' + CAST(Size as nvarchar(5)) + ')' AS DisplayText
FROM FeltModel
ORDER BY Article");

            cbModel.DisplayMember = "DisplayText";
            cbModel.ValueMember = "ModelID";
        }

        private void BtnSuggestAmount_Click(object sender, EventArgs e)
        {
            try
            {
                int modelId = Convert.ToInt32(cbModel.SelectedValue);
                int qty = (int)numQty.Value;

                var dt = Db.Query(@"
SELECT AVG(CASE WHEN ol.Quantity = 0 THEN NULL ELSE p.Amount / ol.Quantity END) AS AvgPerUnit
FROM Payment p
JOIN ClientOrder o ON o.OrderID = p.OrderID
JOIN OrderLine ol ON ol.OrderID = o.OrderID
WHERE ol.ModelID = " + modelId);

                if (dt.Rows.Count > 0 && dt.Rows[0]["AvgPerUnit"] != DBNull.Value)
                {
                    var avgPerUnit = Convert.ToDecimal(dt.Rows[0]["AvgPerUnit"]);
                    numAmount.Value = Math.Min(numAmount.Maximum, decimal.Round(avgPerUnit * qty, 2));
                    lblHint.Text = "Сумма рассчитана по средней цене прошлых продаж.";
                }
                else
                {
                    lblHint.Text = "Для этой модели пока нет истории продаж — введите сумму вручную.";
                }
            }
            catch (Exception ex)
            {
                lblHint.Text = "Не удалось рассчитать сумму: " + ex.Message;
            }
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
                    MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
