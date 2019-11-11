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
    public partial class Login : Form
    {
        public static SqlDataAdapter sda;
        public static string id = "";
        public Login()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\hana\source\repos\Project_v2\Project_v2\Database.mdf;Integrated Security=True"))
            {
                await con.OpenAsync();
                sda = new SqlDataAdapter("Select count(*) from [User] where username ='" + textBox1.Text + "'" +
                        " and password ='" + textBox2.Text + "'", con);
                DataTable dt = new DataTable();

                sda.Fill(dt);
                if (dt.Rows[0][0].ToString() == "1")
                {
                    // MessageBox.Show("Succsess");

                    SqlCommand sc = new SqlCommand("Select Id from [User] where username ='" + textBox1.Text + "'" +
                        " and password ='" + textBox2.Text + "'", con);
                    using (SqlDataReader reader = sc.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            id = String.Format("{0}", reader["Id"]);
                        }
                    }
                    AccountForm f = new AccountForm();
                    this.Hide();
                    f.ShowDialog();
                    this.Close();
                    
                }
                else
                {
                    MessageBox.Show("Invalid credentials");
                }
            }


        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            Register r = new Register();
            r.ShowDialog();
            this.Close();
        }
    }
}
