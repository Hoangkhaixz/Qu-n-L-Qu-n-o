using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClothesBadmintonManagent
{
    public partial class Form6 : Form
    {
        private int CustomerID;
        private SqlConnection con;
        private SqlCommand cmd;
        private SqlDataAdapter adt;
        private DataTable dt = new DataTable();
        private string connectionString = @"Data Source=LAPTOP-I70VJAFS\SQLEXPRESS;Initial Catalog=ASM2;Integrated Security=True;TrustServerCertificate=True";

        public Form6(int customerID)
        {
            InitializeComponent();
            CustomerID = customerID;
            LoadCustomerName();
            LoadProducts();
        }
        private void LoadCustomerName()
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT CustomerName FROM Customer WHERE CustomerID = @CustomerID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerID", CustomerID);
                string customerName = command.ExecuteScalar().ToString();
                txtB_nameCustomer.Text = "Hi, " + customerName + " Welcome to CLothesBadminton..!";
            }
        }
        private void Form6_Load(object sender, EventArgs e)
        {
            con = new SqlConnection(connectionString); // Khởi tạo kết nối với cơ sở dữ liệu
            try
            {
                con.Open(); // Mở kết nối
                cmd = new SqlCommand("SELECT * FROM Products", con); // Tạo lệnh SQL để lấy tất cả sản phẩm
                adt = new SqlDataAdapter(cmd); // Tạo SqlDataAdapter từ lệnh
                adt.Fill(dt); // Điền dữ liệu vào DataTable
                GrView_spUser.DataSource = dt; // Gán DataTable cho DataGridView
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // Hiển thị thông báo lỗi nếu có
            }
            finally
            {
                con.Close(); // Đóng kết nối
            }
        }
        private void GrView_spUser_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Kiểm tra nếu chỉ số dòng hợp lệ
            {
                // Lấy dòng được chọn
                DataGridViewRow selectedRow = GrView_spUser.Rows[e.RowIndex];

                // Hiển thị dữ liệu trong TextBox và ComboBox
                try
                {
                    txtB_idSP.Text = selectedRow.Cells["ProductID"].Value.ToString();
                    txtB_nameSP.Text = selectedRow.Cells["ProductName"].Value.ToString();
                    cbB_sizeSP.SelectedItem = selectedRow.Cells["SizeProduct"].Value.ToString(); // Hiển thị size trong ComboBox
                    txtB_sellSP.Text = selectedRow.Cells["SellingPrice"].Value.ToString();
                    txtB_inventorySP.Text = selectedRow.Cells["InventoryPrice"].Value.ToString();

                    // Hiển thị hình ảnh trong PictureBox
                    if (selectedRow.Cells["ProductImage"].Value != DBNull.Value)
                    {
                        byte[] imageData = (byte[])selectedRow.Cells["ProductImage"].Value; // Lấy dữ liệu hình ảnh
                        if (imageData != null && imageData.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream(imageData)) // Chuyển đổi byte array thành hình ảnh
                            {
                                picB_image.Image = Image.FromStream(ms);
                            }
                        }
                        else
                        {
                            picB_image.Image = null; // Hoặc hình ảnh mặc định
                        }
                    }
                    else
                    {
                        picB_image.Image = null; // Hoặc hình ảnh mặc định
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi: " + ex.Message); // Hiển thị thông báo lỗi nếu có
                }
            }
        }
        private void btn_search_Click(object sender, EventArgs e)
        {
            string productId = txtB_searchSP.Text.Trim(); // Lấy ID sản phẩm từ TextBox

            if (!string.IsNullOrEmpty(productId)) // Kiểm tra ID không rỗng
            {
                using (SqlConnection con = new SqlConnection(connectionString)) // Tạo kết nối mới
                {
                    try
                    {
                        con.Open(); // Mở kết nối
                        string query = "SELECT ProductID, ProductName, SizeProduct, InventoryPrice, SellingPrice, ProductImage FROM Products WHERE ProductID = @ProductID";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", productId); // Thêm tham số ProductID
                            SqlDataReader reader = cmd.ExecuteReader(); // Thực thi truy vấn

                            if (reader.Read()) // Nếu có dữ liệu trả về
                            {
                                string id = reader["ProductID"].ToString();
                                string name = reader["ProductName"].ToString();
                                string size = reader["SizeProduct"].ToString();
                                decimal inventoryQuantity = reader.GetDecimal(reader.GetOrdinal("InventoryPrice"));
                                decimal sellingPrice = reader.GetDecimal(reader.GetOrdinal("SellingPrice"));

                                // Hiển thị thông tin sản phẩm
                                MessageBox.Show($"Thông tin sản phẩm:\nID: {id}\nTên: {name}\nKích thước: {size}\nSố lượng tồn kho: {inventoryQuantity}\nGiá bán: {sellingPrice:C}");
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy sản phẩm với ID: " + productId);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Database error: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng nhập ID sản phẩm để tìm kiếm.");
            }
        }

        private void btn_pay_Click(object sender, EventArgs e)
        {
            if (GrView_spUser.SelectedRows.Count > 0) // Kiểm tra xem có dòng nào được chọn không
            {
                var selectedProductRow = GrView_spUser.SelectedRows[0];
                string productId = selectedProductRow.Cells["ProductID"].Value.ToString();
                string productname = selectedProductRow.Cells["ProductName"].Value.ToString();
                decimal sellingPrice = Convert.ToDecimal(selectedProductRow.Cells["SellingPrice"].Value);
                decimal inventoryQuantity = Convert.ToDecimal(selectedProductRow.Cells["InventoryPrice"].Value);

                if (inventoryQuantity <= 0)
                {
                    MessageBox.Show("Sản phẩm đã hết hàng. Không thể thanh toán.");
                    return;
                }

                if (!int.TryParse(txtB_Quantity.Text, out int quantitySold) || quantitySold <= 0)
                {
                    MessageBox.Show("Vui lòng nhập số lượng bán ra hợp lệ.");
                    return;
                }

                if (quantitySold > inventoryQuantity)
                {
                    MessageBox.Show("Số lượng bán ra không được vượt quá số lượng tồn kho.");
                    btn_pay.Enabled = false;
                    return;
                }

                decimal totalPrice = sellingPrice * quantitySold;
                DateTime saleDate = DateTime.Now;
                DialogResult result = MessageBox.Show($"Bạn có chắc chắn muốn thanh toán cho sản phẩm:\n\nID: {productId}\n Name: {productname}\n Giá: {sellingPrice:C}\nSố lượng tồn kho: {inventoryQuantity}\nTotal: {totalPrice:C}\n\nNhấn OK để xác nhận.", "Xác Nhận Thanh Toán", MessageBoxButtons.OKCancel);

                if (result == DialogResult.OK)
                {
                    MessageBox.Show("Thanh toán thành công cho sản phẩm: " + productname);
                    UpdateInventory(productId, inventoryQuantity - quantitySold);                 
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm để thanh toán.");
            }
        }
        private void UpdateInventory(string productId, decimal newQuantity)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Products SET InventoryPrice = @NewQuantity WHERE ProductID = @ProductID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NewQuantity", newQuantity);
                    command.Parameters.AddWithValue("@ProductID", productId);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void LoadProducts()
        {
            dt.Clear();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    cmd = new SqlCommand("SELECT * FROM Products", con);
                    adt = new SqlDataAdapter(cmd);
                    adt.Fill(dt);
                    GrView_spUser.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
