using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_v2
{
    public partial class PayBalance : Form
    {
        public static Int32 newBal;
        public PayBalance()
        {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!Char.IsDigit(c) && c != 8)
            {
                e.Handled = true;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            bool empty = true;
            if (String.IsNullOrEmpty(textBox1.Text)) empty = false;

            String connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\hana\source\repos\Project_v2\Project_v2\Database.mdf;Integrated Security=True";
            if (empty)
            {
                newBal = AccountForm.balance-Convert.ToInt32(textBox1.Text);
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        SqlCommand da = new SqlCommand("Update [Account] set balance = @balance where Id = @accId", conn);
                        da.Parameters.AddWithValue("@accId", AccountForm.accID);
                        da.Parameters.AddWithValue("@balance", newBal);
                        da.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    Dispose();
                    Close();
                }
            }
            else MessageBox.Show("Enter $");
        }
    }
}
