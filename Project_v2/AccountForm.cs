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
using static System.Windows.Forms.DataFormats;

namespace Project_v2
{
    public partial class AccountForm : Form
    {
        public static Int32 userId;
        public static Int32 accID;
        public static Int32 balance;
        private static bool accExists;
        private static IDictionary<int, int> accList;
        public static DataClasses1DataContext db;

        public AccountForm()
        {
            InitializeComponent();
        }
        //Users can check their account balances and pay any amount (even more than what they owe).

        private void AccountForm_Load(object sender, EventArgs e)
        {
            db = new DataClasses1DataContext();
            label15.Visible = false;

            if (Login.id != "")
            {
                userId = Convert.ToInt32(Login.id);
                label2.Text = Login.id;
            }
            else
            {
                userId = Convert.ToInt32(Register.id);
                label2.Text = Register.id;
            }
            numericUpDown1.Value = accID;
            CountAccAmount();
            label5.Hide();   
        }

        private async void CountAccAmount()
        {
            accList = new Dictionary<int, int>();
            int total = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\hana\source\repos\Project_v2\Project_v2\Database.mdf;Integrated Security=True"))
                {
                    await con.OpenAsync();
                    using (SqlCommand sda = new SqlCommand($"Select Id from [Account] where userID =@userid", con))
                    {
                        sda.Parameters.Add(new SqlParameter("@userid", AccountForm.userId));
                        using (SqlDataReader reader = sda.ExecuteReader())
                        {
                            int i = 1;
                            while (reader.Read())
                            {
                                
                                accList.Add(i, Convert.ToInt32(reader["Id"]));
                               
                                i++;
                            }
                            
                        }
                    }
                    /*foreach (KeyValuePair<int, int> kvp in accList)
                    {
                        MessageBox.Show($"Key: {kvp.Key},Actial Value: {kvp.Value}");
                    }*/
                    using (SqlCommand sda = new SqlCommand($"Select count(*) from [Account] where userID =@userid", con))
                    {
                        
                        sda.Parameters.Add(new SqlParameter("@userid",AccountForm.userId));
                        total = (int)sda.ExecuteScalar();
                       
                        if (total > 0)
                        {  
                            numericUpDown1.Minimum = 1;
                            numericUpDown1.Maximum = accList.Keys.Last();    
                        }
                        else numericUpDown1.Maximum = 1;
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AddAccountForm acf = new AddAccountForm();
            acf.ShowDialog();
            //To refresh list of available accounts: 
            CountAccAmount();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int val = Convert.ToInt32(numericUpDown1.Value);
            if (!accList.Keys.Contains(val))
            {
                //accID = 1; // by default 
                MessageBox.Show("Must pick a valid account # or create new");
                accExists = false;
            }
            else
            {
                foreach (var acc in accList)
                {
                    if (acc.Key == val)
                    {
                        accID = acc.Value;
                        accExists = true;
                    }
                }
                MessageBox.Show($"Account choosen #{val}");
            }

            ShowAccBalance();
        }

        private async void ShowAccBalance()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\hana\source\repos\Project_v2\Project_v2\Database.mdf;Integrated Security=True"))
                {

                    SqlCommand sda = new SqlCommand("Select balance from [Account] where Id = @accID and userID = @userID", con);
                    sda.Parameters.AddWithValue("@accID", accID);
                    sda.Parameters.AddWithValue("@userID", userId);
                    await con.OpenAsync();
                    using (SqlDataReader reader = sda.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            label5.Text = String.Format("{0}", reader["balance"]);
                            balance = Convert.ToInt32(reader["balance"]);
                            label5.Show();
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PayBalance pb = new PayBalance();
            pb.ShowDialog();
            label5.Text = Convert.ToString(PayBalance.newBal);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            if (accExists == true)
            {
                db = new DataClasses1DataContext();
                int low = int.Parse(textBox1.Text) - 1;
                int up = int.Parse(textBox2.Text) + 1;
                int counter = 0;

                db.Connection.Open();
                var priceProducts = (from tP in db.Products
                                     where tP.productPrice > low && tP.productPrice < up
                                     join ci in db.CartItems on tP.Id equals ci.productID
                                     join c in db.Carts.Where(s => s.accountID == accID) on ci.cartID equals c.Id
                                     select tP).OrderBy(a => a.Id).ToList();

                if (priceProducts.Count > 0)
                {
                    foreach (var c in priceProducts)
                    {
                        counter++;
                        listBox1.Items.Add($"{counter}. {c.productName} ${c.productPrice}");
                    }
                    label15.Text = Convert.ToString(counter);
                    label15.Show();
                    //At the end must empty the list
                    priceProducts = null;
                    db.Connection.Close();
                }
                else MessageBox.Show("No products at this price range found");
            }
            else MessageBox.Show("Please choose account #");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            //string low = dateTimePicker1.Value.ToShortDateString();
            //string up = dateTimePicker2.Value.ToShortDateString();
            //MessageBox.Show($"Date from {low} to {up}");
            if (accExists == true)
            {
                DateTime l = dateTimePicker1.Value.Date;
                DateTime u = dateTimePicker2.Value.Date;
                int counter = 0;

                if (l.CompareTo(u) > 0)
                {
                    MessageBox.Show("Wrong dates");
                }
                else
                {
                    db = new DataClasses1DataContext();
                    db.Connection.Open();
                    listBox1.Items.Clear();
                    var dateProducts = (from tP in db.Products
                                        join ci in db.CartItems on tP.Id equals ci.productID
                                        join c in db.Carts.Where(x => x.dateCreated > l.AddDays(-1) && x.dateCreated < u.AddDays(1) && x.accountID == accID)
                                        on ci.cartID equals c.Id
                                        select tP).OrderBy(a => a.Id);

                    foreach (var c in dateProducts)
                    {
                        counter++;
                        listBox1.Items.Add($"{counter}. {c.productName} ${c.productPrice}");
                    }
                    label15.Text = Convert.ToString(counter);
                    label15.Show();
                    if (counter < 1)
                    {
                        MessageBox.Show("No products were purchased for these dates");
                    }
                    dateProducts = null;
                    db.Connection.Close();
                }
            }
            else MessageBox.Show("Please choose account #");
        }

        private void storeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (accExists == true)
            {
                Hide();
                Form1 form1 = new Form1();
                form1.ShowDialog();
                form1 = null;
                Show();
            }
            else MessageBox.Show("Account must be choosen");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!Char.IsDigit(c) && c != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!Char.IsDigit(c) && c != 8)
            {
                e.Handled = true;
            }
        }
    }
}
