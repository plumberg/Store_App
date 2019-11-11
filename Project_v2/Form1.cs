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
    public partial class Form1 : Form
    {
        public static DataClasses1DataContext db;
        public static int cartID, prevCartTotalPrice;
        //private SqlConnection con;
        private static bool cartCreated;
        private Int32 priceSum;
        List<int> index;
        Dictionary<int, string> prodList;


        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            //con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\hana\source\repos\Project_v2\Project_v2\Database.mdf;Integrated Security=True");
            db = new DataClasses1DataContext();
            priceSum = 0;
            // cartCreated = false;
            label4.Visible = false;
            label5.Visible = false;

            await db.Connection.OpenAsync();
            var prod = db.Products;
            index = new List<int>();
            prodList = new Dictionary<int, string>();
            foreach (var v in prod)
            {
                index.Add(v.Id);
                //prodList.Add($"{v.productName}", Convert.ToInt32(v.productPrice));
                prodList.Add(Convert.ToInt32(v.Id), $"{v.productName}");

            }
            List<Int32> prices = ProductPrices();
            foreach (var item in prodList)
            {
                int fp = item.Key;
                productsCheckedListBox.Items.Add(item.Key + ". " + item.Value + " $"+ prices.ElementAt(fp-1));
            }

            db.Connection.Close();

        }

        private List<int> ProductPrices()
        {
            try
            {
                    //Lets find prices for each checked product in the cart
                    var prices = from p in db.Products
                                 select Convert.ToInt32(p.productPrice);

                return prices.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return null;

        }

        private void storeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("You are in a store");
        }

        private void accountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //hide();
            db.Dispose();
            Dispose();
            
            AccountForm form1 = new AccountForm();
            form1.ShowDialog();
            form1 = null;

            //Show();
            Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            db.Dispose();
            Dispose();
        }

        private async void addToCartBtn_Click(object sender, EventArgs e)
        {
           
            foreach (var v in productsCheckedListBox.CheckedItems)
            {
                cartCheckedListBox.Items.Add(v);
            }
            label5.Visible = true;
            label5.Text = $"Total items: {cartCheckedListBox.Items.Count}";


                // Creating Cart here with Add button. Need to check that the new cart can be created only once per day.
                try
                {
                    using (SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\hana\source\repos\Project_v2\Project_v2\Database.mdf;Integrated Security=True"))
                    {
                        await con.OpenAsync();
                        SqlCommand sda;
                        DateTime currDate = DateTime.Now.Date;

                        if (cartCreated == false)
                        {
                            //not allowing for user to use more than 1 cart per day. 
                            //If user uses different accounts to buy products, the cart will stay the same but the balance will be updated for specific account
                            string sql1 = "IF NOT EXISTS (SELECT * FROM CART WHERE accountID = @param1 and dateCreated = @param2) " +
                            "INSERT INTO Cart(accountID,dateCreated) VALUES(@param1,@param2)";
                            using (sda = new SqlCommand(sql1, con))
                            {
                                sda.Parameters.AddWithValue("@param1", AccountForm.accID);
                                sda.Parameters.AddWithValue("@param2", currDate.ToShortDateString());
                                sda.ExecuteNonQuery();
                            }
                            cartCreated = true;
                           // MessageBox.Show($"A new cart for {currDate.ToShortDateString()} was created");
                        }

                        //now find the id of the today's cart
                        sda = new SqlCommand("Select Id, totalPrice from [Cart] where accountID ='" + AccountForm.accID + "'" +
                             " and dateCreated ='" + currDate + "'", con);
                        using (SqlDataReader reader = sda.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cartID = reader.GetInt32(0);
                                prevCartTotalPrice = reader.GetInt32(1);
                            }
                            MessageBox.Show($"Cart Id is {cartID}");
                        }
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            

        }

        private void removeFromCartBtn_Click(object sender, EventArgs e)
        {
                for (int i = 0; i < cartCheckedListBox.Items.Count; i++)
            {
                cartCheckedListBox.Items.Remove(cartCheckedListBox.CheckedItems[i]);
            }
            if (cartCheckedListBox.CheckedItems == null) label5.Visible = false;
            else
                label5.Text = $"Total items: {cartCheckedListBox.Items.Count}";
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (cartCheckedListBox.CheckedItems.Count > 0)
            {
                List<Int32> productIds = ProductIds();
                Int32 priceSumAcc = RefreshAddTotalPrice(productIds);
                //priceSum += RefreshAddTotalPrice(productIds);
                prevCartTotalPrice += RefreshAddTotalPrice(productIds);
                //700 - balance is what balance $ will remain left  
                if (priceSumAcc <= 700 - AccountForm.balance)
                {
                    using (var con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\hana\source\repos\Project_v2\Project_v2\Database.mdf;Integrated Security=True"))
                    {
                        try
                        {
                            await con.OpenAsync();
                            //Now we can insert into cart total price and items to the cart items table
                            string sql2 = "INSERT INTO CartItems(productID, cartID) VALUES (@product,@cart)";
                            using (var sda = new SqlCommand(sql2, con))
                            {
                                foreach (int i in productIds)
                                {
                                    sda.Parameters.Clear();
                                    sda.Parameters.AddWithValue("@product", i);
                                    sda.Parameters.AddWithValue("@cart", cartID);
                                    sda.ExecuteNonQuery();
                                }
                                MessageBox.Show($"The items were added to the cart #{cartID}");
                            }

                            string sql3 = "update Cart set totalPrice = @price where Id = @cartID";
                            using (var sda = new SqlCommand(sql3, con))
                            {
                                sda.Parameters.AddWithValue("@price", prevCartTotalPrice);
                                sda.Parameters.AddWithValue("@cartID", cartID);
                                sda.ExecuteNonQuery();
                            }

                            UpdateAccBalance(con, priceSumAcc);

                            con.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            con.Close();
                        }
                    }
                }
                else MessageBox.Show("Your account balance was exceeded. Not allowed to owe more than $700. Please pay on your balance");
            }
            else MessageBox.Show("Please select items from a cart");
        }

        private void UpdateAccBalance(SqlConnection con, int priceSum)
        {
            int bal=0;
            int id = AccountForm.accID;
            SqlCommand sda = new SqlCommand("select balance from Account where Id = '" + AccountForm.accID + "'", con);
            using (SqlDataReader reader = sda.ExecuteReader())
            {
                if (reader.Read())
                {
                    bal = reader.GetInt32(0);
                }
            }
            bal += priceSum;
            string sql = "update Account set balance = @bal where Id = @id";
            using (sda = new SqlCommand(sql, con))
            {
                sda.Parameters.AddWithValue("@bal",bal);
                sda.Parameters.AddWithValue("@id", id);
                sda.ExecuteNonQuery();
                MessageBox.Show("Account balance was updated");
            }
        }

        private int RefreshAddTotalPrice(List<Int32> productIds)
        {
            int priceSum = 0;

                try
                {
                    priceSum = 0;
                    db.Connection.Open();
                    foreach (int i in productIds)
                    {
                        //Lets find prices for each checked product in the cart
                        var prices = from p in db.Products
                                     where p.Id == i
                                     select Convert.ToInt32(p.productPrice);
                        priceSum += prices.Sum();
                    }
                    label4.Visible = true;
                    label4.Text = $"$ {priceSum}";

                    db.Connection.Close();
                    return priceSum;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {

                    db.Connection.Close();
                }
            return priceSum;
        }

        private List<Int32> ProductIds()
        {
            List<Int32> productIds = new List<Int32>();
            string[] split = new string[] { };
            //Lets find all id's for checked products of the card and add it to the list
            foreach (string v in cartCheckedListBox.CheckedItems)
            {
                split = v.Split('.');
                int num = Convert.ToInt32(split[0]);
                productIds.Add(num);
            }
            return productIds;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (cartCheckedListBox.CheckedItems.Count > 0)
            {
                List<Int32> productIds = ProductIds();
                RefreshAddTotalPrice(productIds);
            }
            else MessageBox.Show("Please check desired items from the cart");
        }
    }
}
