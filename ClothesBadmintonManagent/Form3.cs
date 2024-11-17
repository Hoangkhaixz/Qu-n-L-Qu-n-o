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

namespace ClothesBadmintonManagent
{
    public partial class Form3 : Form
    {
        string connectstring = @"Data Source=LAPTOP-I70VJAFS\SQLEXPRESS;Initial Catalog=ASM2;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter adt;
        DataTable dt = new DataTable();
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            con = new SqlConnection(connectstring);
            try
            {
                con.Open();
                cmd = cmd = new SqlCommand("select * from Customer", con);
                adt = new SqlDataAdapter(cmd);
                adt.Fill(dt);
                grView_hienthi.DataSource = dt;
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void grView_hienthi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure that the click is on a valid row (not header or empty)
            if (e.RowIndex >= 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = grView_hienthi.Rows[e.RowIndex];

                // Retrieve values from the selected row and display them on the form
                txtB_idCustomer.Text = selectedRow.Cells[0].Value.ToString();         // ID Customer
                txtB_nameCustomer.Text = selectedRow.Cells[1].Value.ToString();       // Name Customer
                if (selectedRow.Cells[2].Value.ToString() == "FEMALE")
                {
                    rad_fema.Checked = true;  // Set Male gender radio button
                }
                else if (selectedRow.Cells[2].Value.ToString() == "MALE")
                {
                    rad_male.Checked = true;  // Set Female gender radio button
                }
                txtB_dateCustomer.Text = selectedRow.Cells[3].Value.ToString();        // Date of Birth
                txtB_phoneCustomer.Text = selectedRow.Cells[4].Value.ToString();      // Phone Number
                txtB_emailCustomer.Text = selectedRow.Cells[5].Value.ToString();      // Email
            }
        }

        private void btn_addCustomer_Click(object sender, EventArgs e)
        {
            string CustomerID = txtB_idCustomer.Text;
            string CustomerName = txtB_nameCustomer.Text;
            string Gender = (rad_fema.Checked) ? "FEMALE" : "MALE";
            DateTime BirthofDate;
            string Phone = txtB_phoneCustomer.Text;
            string Gmail = txtB_emailCustomer.Text;

            // Chuyển đổi chuỗi ngày thành kiểu DateTime
            if (!DateTime.TryParse(txtB_dateCustomer.Text, out BirthofDate))
            {
                MessageBox.Show("Invalid date format. Please use yyyy-MM-dd.");
                return;
            }

            string insertQuery = "INSERT INTO Customer (CustomerID, CustomerName, Gender, BirthofDate, Phone, Gmail) VALUES (@CustomerID, @CustomerName, @Gender, @BirthofDate, @Phone, @Gmail)";

            using (SqlCommand cmd = new SqlCommand(insertQuery, con))
            {
                cmd.Parameters.AddWithValue("@CustomerID", CustomerID);
                cmd.Parameters.AddWithValue("@CustomerName", CustomerName);
                cmd.Parameters.AddWithValue("@Gender", Gender);
                cmd.Parameters.AddWithValue("@BirthofDate", BirthofDate);
                cmd.Parameters.AddWithValue("@Phone", Phone);
                cmd.Parameters.AddWithValue("@Gmail", Gmail);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                // Refresh the DataGridView
                LoadCustomerData();
            }
        }
        private void LoadCustomerData()
        {
            dt.Clear();
            adt.Fill(dt);
            grView_hienthi.DataSource = dt;
        }

        private void btn_updateCustomer_Click(object sender, EventArgs e)
        {
            if (grView_hienthi.SelectedRows.Count > 0)
            {
                string CustomerID = txtB_idCustomer.Text;
                string CustomerName = txtB_nameCustomer.Text;
                string Gender = (rad_fema.Checked) ? "FEMALE" : "MALE";
                DateTime BirthofDate;
                string Phone = txtB_phoneCustomer.Text;
                string Gmail = txtB_emailCustomer.Text;

                // Chuyển đổi chuỗi ngày thành kiểu DateTime
                if (!DateTime.TryParse(txtB_dateCustomer.Text, out BirthofDate))
                {
                    MessageBox.Show("Invalid date format. Please use yyyy-MM-dd.");
                    return;
                }

                string updateQuery = "UPDATE Customer SET CustomerName = @CustomerName, Gender = @Gender, BirthofDate = @BirthofDate, Phone = @Phone, Gmail = @Gmail WHERE CustomerID = @CustomerID";

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", CustomerID);
                    cmd.Parameters.AddWithValue("@CustomerName", CustomerName);
                    cmd.Parameters.AddWithValue("@Gender", Gender);
                    cmd.Parameters.AddWithValue("@BirthofDate", BirthofDate);
                    cmd.Parameters.AddWithValue("@Phone", Phone);
                    cmd.Parameters.AddWithValue("@Gmail", Gmail);

                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();

                        // Làm mới DataGridView
                        LoadCustomerData();
                        MessageBox.Show("Customer updated successfully!");
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("SQL Error: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a row to update");
            }
        }

        private void btn_deleCustomer_Click(object sender, EventArgs e)
        {
            if (grView_hienthi.SelectedRows.Count > 0)
            {
                // Lấy giá trị CustomerID của dòng được chọn
                string CustomerID = grView_hienthi.SelectedRows[0].Cells[0].Value.ToString();

                // Xác nhận trước khi xóa
                var confirmResult = MessageBox.Show("Are you sure you want to delete this customer?", "Confirm Delete", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    // Tạo câu lệnh SQL DELETE
                    string deleteQuery = "DELETE FROM Customer WHERE CustomerID = @CustomerID";

                    using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", CustomerID);

                        try
                        {
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();

                            // Làm mới lại DataGridView
                            LoadCustomerData();
                            MessageBox.Show("Customer deleted successfully!");
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show("SQL Error: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a row to delete");
            }
        }

        private void btn_exitCustomer_Click(object sender, EventArgs e)
        {
            DialogResult exit = MessageBox.Show("Are you want to exit..?",
               "Warning",
               MessageBoxButtons.YesNo,
               MessageBoxIcon.Question);

            if (exit == DialogResult.No)
            {
                MessageBox.Show("STAY..!");
            }
            else
            {
                Hide();
            }
        }
    }
}
