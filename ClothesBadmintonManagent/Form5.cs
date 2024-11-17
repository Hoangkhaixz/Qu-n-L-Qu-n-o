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
    public partial class Form5 : Form
    {
        // Chuỗi kết nối đến cơ sở dữ liệu
        string connectstring = @"Data Source=LAPTOP-I70VJAFS\SQLEXPRESS;Initial Catalog=ASM2;Integrated Security=True;TrustServerCertificate=True";
        SqlConnection con; // Đối tượng kết nối SQL
        SqlCommand cmd; // Đối tượng lệnh SQL
        SqlDataAdapter adt; // Đối tượng để lấy dữ liệu từ DB
        DataTable dt = new DataTable(); // Bảng dữ liệu để lưu trữ sản phẩm
        public Form5()
        {
            InitializeComponent();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            con = new SqlConnection(connectstring); // Khởi tạo kết nối với cơ sở dữ liệu
            try
            {
                con.Open(); // Mở kết nối
                cmd = new SqlCommand("select * from Products", con); // Tạo lệnh SQL để lấy tất cả sản phẩm
                adt = new SqlDataAdapter(cmd); // Tạo SqlDataAdapter từ lệnh
                adt.Fill(dt); // Điền dữ liệu vào DataTable
                GridV_hienthi6.DataSource = dt; // Gán DataTable cho DataGridView
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

        private void GridV_hienthi6_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Kiểm tra nếu chỉ số dòng hợp lệ
            {
                // Lấy dòng được chọn
                DataGridViewRow selectedRow = GridV_hienthi6.Rows[e.RowIndex];

                // Hiển thị dữ liệu trong TextBox
                try
                {
                    // Gán giá trị từ dòng được chọn vào các TextBox tương ứng
                    txtB_idProduct.Text = selectedRow.Cells["ProductID"].Value.ToString();
                    txtB_nameProduct.Text = selectedRow.Cells["ProductName"].Value.ToString();
                    cbB_sizeProduct.Text = selectedRow.Cells["SizeProduct"].Value.ToString();
                    txtB_priceProduct.Text = selectedRow.Cells["SellingPrice"].Value.ToString();
                    txtB_inventProduct.Text = selectedRow.Cells["InventoryPrice"].Value.ToString();

                    // Hiển thị hình ảnh trong PictureBox
                    if (selectedRow.Cells["ProductImage"].Value != DBNull.Value)
                    {
                        byte[] imageData = (byte[])selectedRow.Cells["ProductImage"].Value; // Lấy dữ liệu hình ảnh
                        if (imageData != null && imageData.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream(imageData)) // Chuyển đổi byte array thành hình ảnh
                            {
                                picB_imageProduct.Image = Image.FromStream(ms);
                            }
                        }
                        else
                        {
                            picB_imageProduct.Image = null; // Hoặc hình ảnh mặc định
                        }
                    }
                    else
                    {
                        picB_imageProduct.Image = null; // Hoặc hình ảnh mặc định
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi: " + ex.Message); // Hiển thị thông báo lỗi nếu có
                }
            }
        }

        private void btn_addProduct_Click(object sender, EventArgs e)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtB_nameProduct.Text) ||
                string.IsNullOrWhiteSpace(cbB_sizeProduct.Text) ||
                string.IsNullOrWhiteSpace(txtB_idProduct.Text) || // Thêm kiểm tra cho ProductID
                !decimal.TryParse(txtB_priceProduct.Text, out decimal SellingPrice) ||
                !decimal.TryParse(txtB_inventProduct.Text, out decimal InventoryPrice))
            {
                MessageBox.Show("Please enter valid values for Product ID, Product Name, Size, Inventory Price, and Selling Price.");
                return; // Thoát nếu dữ liệu không hợp lệ
            }

            // Chuyển đổi đường dẫn hình ảnh thành mảng byte
            byte[] productImage = PathToByteArray(this.Text); // Đảm bảo this.Text chứa đường dẫn hình ảnh

            using (SqlConnection con = new SqlConnection(connectstring)) // Tạo kết nối mới
            {
                try
                {
                    con.Open(); // Mở kết nối

                    // Tạo câu lệnh INSERT để thêm sản phẩm mới
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Products (ProductID, ProductName, SizeProduct, ProductImage, InventoryPrice, SellingPrice) VALUES (@ProductID, @ProductName, @SizeProduct, @ProductImage, @InventoryPrice, @SellingPrice)", con))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", int.Parse(txtB_idProduct.Text)); // Lấy giá trị ProductID từ textbox
                        cmd.Parameters.AddWithValue("@ProductName", txtB_nameProduct.Text); // Lấy tên sản phẩm
                        cmd.Parameters.AddWithValue("@SizeProduct", cbB_sizeProduct.Text); // Lấy kích thước sản phẩm
                        cmd.Parameters.AddWithValue("@ProductImage", productImage); // Lấy hình ảnh
                        cmd.Parameters.AddWithValue("@InventoryPrice", InventoryPrice); // Lấy số lượng tồn kho
                        cmd.Parameters.AddWithValue("@SellingPrice", SellingPrice); // Lấy giá bán

                        cmd.ExecuteNonQuery(); // Thực thi câu lệnh INSERT
                    }

                    MessageBox.Show("Product added successfully!"); // Thông báo thành công
                    LoadProducts(); // Tải lại danh sách sản phẩm
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}"); // Hiển thị lỗi cơ sở dữ liệu nếu có
                }
            }
        }
        // Chuyển đổi file hình ảnh thành mảng byte
        byte[] PathToByteArray(string path)
        {
            MemoryStream m = new MemoryStream(); // Tạo MemoryStream để lưu trữ hình ảnh
            Image img = Image.FromFile(path); // Tải hình ảnh từ file
            img.Save(m, System.Drawing.Imaging.ImageFormat.Png); // Lưu hình ảnh vào MemoryStream
            return m.ToArray(); // Trả về mảng byte
        }

        private void LoadProducts()
        {
            dt.Clear(); // Xóa dữ liệu cũ trong DataTable
            using (SqlConnection con = new SqlConnection(connectstring)) // Tạo kết nối mới
            {
                try
                {
                    con.Open(); // Mở kết nối
                    cmd = new SqlCommand("SELECT * FROM Products", con); // Tạo lệnh SQL để lấy tất cả sản phẩm
                    adt = new SqlDataAdapter(cmd); // Tạo SqlDataAdapter từ lệnh
                    adt.Fill(dt); // Điền dữ liệu vào DataTable
                    GridV_hienthi6.DataSource = dt; // Gán DataTable cho DataGridView
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message); // Hiển thị thông báo lỗi nếu có
                }
            }
        }

        private void picB_imageProduct_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog(); // Tạo hộp thoại mở file
            if (open.ShowDialog() == DialogResult.OK) // Kiểm tra xem người dùng đã chọn file chưa
            {
                picB_imageProduct.Image = Image.FromFile(open.FileName); // Hiển thị hình ảnh đã chọn
                this.Text = open.FileName; // Hiển thị đường dẫn file cho mục đích gỡ lỗi
            }
        }
        private void btn_updProduct_Click(object sender, EventArgs e)
        {
            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtB_nameProduct.Text) ||
                string.IsNullOrWhiteSpace(cbB_sizeProduct.Text) ||
                string.IsNullOrWhiteSpace(txtB_idProduct.Text) || // Kiểm tra ProductID
                !decimal.TryParse(txtB_priceProduct.Text, out decimal SellingPrice) ||
                !decimal.TryParse(txtB_inventProduct.Text, out decimal InventoryPrice))
            {
                MessageBox.Show("Please enter valid values for Product ID, Product Name, Size, Inventory Price, and Selling Price.");
                return; // Thoát nếu dữ liệu không hợp lệ
            }

            // Chuyển đổi đường dẫn hình ảnh thành mảng byte
            byte[] productImage = PathToByteArray(this.Text); // Đảm bảo this.Text chứa đường dẫn hình ảnh

            using (SqlConnection con = new SqlConnection(connectstring)) // Tạo kết nối mới
            {
                try
                {
                    con.Open(); // Mở kết nối

                    // Tạo câu lệnh UPDATE để sửa đổi thông tin sản phẩm
                    using (SqlCommand cmd = new SqlCommand("UPDATE Products SET ProductName = @ProductName, SizeProduct = @SizeProduct, ProductImage = @ProductImage, InventoryPrice = @InventoryPrice, SellingPrice = @SellingPrice WHERE ProductID = @ProductID", con))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", int.Parse(txtB_idProduct.Text)); // Lấy giá trị ProductID từ textbox
                        cmd.Parameters.AddWithValue("@ProductName", txtB_nameProduct.Text); // Lấy tên sản phẩm
                        cmd.Parameters.AddWithValue("@SizeProduct", cbB_sizeProduct.Text); // Lấy kích thước sản phẩm
                        cmd.Parameters.AddWithValue("@ProductImage", productImage); // Lấy hình ảnh
                        cmd.Parameters.AddWithValue("@InventoryPrice", InventoryPrice); // Lấy số lượng tồn kho
                        cmd.Parameters.AddWithValue("@SellingPrice", SellingPrice); // Lấy giá bán

                        cmd.ExecuteNonQuery(); // Thực thi câu lệnh UPDATE
                    }

                    MessageBox.Show("Product updated successfully!"); // Thông báo thành công
                    LoadProducts(); // Tải lại danh sách sản phẩm
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}"); // Hiển thị lỗi cơ sở dữ liệu nếu có
                }
            }
        }
        private void btn_deleProduct_Click(object sender, EventArgs e)
        {
            if (GridV_hienthi6.SelectedRows.Count > 0) // Kiểm tra xem có dòng nào được chọn không
            {
                // Lấy dòng được chọn
                var selectedRow = GridV_hienthi6.SelectedRows[0];
                int productIdToDelete = Convert.ToInt32(selectedRow.Cells["ProductID"].Value); // Lấy ProductID từ dòng đã chọn

                using (SqlConnection con = new SqlConnection(connectstring)) // Tạo kết nối mới
                {
                    try
                    {
                        con.Open(); // Mở kết nối
                                    // Tạo câu lệnh DELETE để xóa sản phẩm
                        using (SqlCommand cmd = new SqlCommand("DELETE FROM Products WHERE ProductID = @ProductID", con))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", productIdToDelete); // Thêm tham số ProductID
                            cmd.ExecuteNonQuery(); // Thực thi câu lệnh DELETE
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Database error: {ex.Message}"); // Hiển thị lỗi cơ sở dữ liệu nếu có
                    }
                }

                // Xóa dòng khỏi DataGridView
                GridV_hienthi6.Rows.RemoveAt(selectedRow.Index);
            }
            else
            {
                MessageBox.Show("Please select a row to delete."); // Thông báo nếu không có dòng nào được chọn
            }
        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            string productId = txtB_search.Text.Trim(); // Lấy ID sản phẩm từ TextBox

            if (!string.IsNullOrEmpty(productId)) // Kiểm tra ID không rỗng
            {
                using (SqlConnection con = new SqlConnection(connectstring)) // Tạo kết nối mới
                {
                    try
                    {
                        con.Open(); // Mở kết nối
                                    // Tạo truy vấn để lấy thông tin sản phẩm theo ProductID
                        string query = "SELECT ProductID, ProductName, SizeProduct, InventoryPrice, SellingPrice, ProductImage FROM Products WHERE ProductID = @ProductID";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", productId); // Thêm tham số ProductID
                            SqlDataReader reader = cmd.ExecuteReader(); // Thực thi truy vấn

                            if (reader.Read()) // Nếu có dữ liệu trả về
                            {
                                // Lấy thông tin sản phẩm
                                string id = reader["ProductID"].ToString();
                                string name = reader["ProductName"].ToString();
                                string size = reader["SizeProduct"].ToString();
                                decimal inventoryQuantity = reader.GetDecimal(reader.GetOrdinal("InventoryPrice")); // Số lượng tồn kho
                                decimal sellingPrice = reader.GetDecimal(reader.GetOrdinal("SellingPrice")); // Giá bán

                                // Hiển thị thông tin sản phẩm
                                MessageBox.Show($"Thông tin sản phẩm:\nID: {id}\nTên: {name}\nKích thước: {size}\nSố lượng tồn kho: {inventoryQuantity}\nGiá bán: {sellingPrice:C}");
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy sản phẩm với ID: " + productId); // Thông báo nếu không tìm thấy
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Database error: {ex.Message}"); // Hiển thị lỗi cơ sở dữ liệu nếu có
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng nhập ID sản phẩm để tìm kiếm."); // Thông báo nếu ID rỗng
            }
        }
    }
}
