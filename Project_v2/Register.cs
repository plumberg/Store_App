using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Sql;
using System.Data.SqlClient;

namespace Project_v2
{
    public partial class Register : Form
    {
        public static SqlConnection con;
        public static string id = "";
        public Register()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String login = textBox1.Text;
            String pass = textBox2.Text;
            con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\hana\source\repos\Project_v2\Project_v2\Database.mdf;Integrated Security=True");
            String query = "Insert into [User] (Username, Password) values (@textBox1, @textBox2)";
           
            try
            {
                con.Open();
                SqlCommand command = new SqlCommand(query, con);
                command.Parameters.AddWithValue("@textBox1", login);
                command.Parameters.AddWithValue("@textBox2", pass);
                command.ExecuteNonQuery();
                MessageBox.Show(login + " added");
                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                con.Close();
                Close();
                Login form = new Login();
                form.ShowDialog();
            }
            // passRegTxtBox.Text;
            //MessageBox.Show("Welconme, "+loginRegTxtBox.Text);
        }
    }
}
