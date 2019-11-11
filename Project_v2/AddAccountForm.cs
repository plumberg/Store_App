using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project_v2
{
    public partial class AddAccountForm : Form
    {
        public AddAccountForm()
        {
            InitializeComponent();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if(!Char.IsDigit(c)&& c != 8)
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
                int bal = -Convert.ToInt32(textBox1.Text);
                
                try
                {
                    //using (OdbcConnection conn = new OdbcConnection(connectionString))
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync();
                        /*  OdbcDataAdapter da = new OdbcDataAdapter($"Insert into [Account] (userID, balance) values (@userid, @balance)", conn);
                          da.InsertCommand.Parameters.AddWithValue("@userid", AccountForm.ids);
                          da.InsertCommand.Parameters.AddWithValue("@balance", bal);
                         */
                        SqlCommand da = new SqlCommand($"Insert into [Account] (userID, balance) values (@userid, @balance)", conn);
                        da.Parameters.AddWithValue("@userid", AccountForm.userId);
                        da.Parameters.AddWithValue("@balance", bal);
                        da.ExecuteNonQuery();

                        MessageBox.Show($"For user {AccountForm.userId} opened account with ${bal}");
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
            else MessageBox.Show("Enter first deposit $");
            
        }

    }
}
