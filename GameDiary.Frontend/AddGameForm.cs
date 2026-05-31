namespace GameDiary.Frontend
{
    public partial class AddGameForm : Form
    {
        public string GameTitle { get; private set; } = "";
        public string Platform { get; private set; } = "";
        public string Status { get; private set; } = "";

        public AddGameForm()
        {
            InitializeComponent();

            cmbPlatform.Items.AddRange(new[] { "PC", "PS5", "PS4", "Xbox", "Nintendo Switch" });
            cmbPlatform.SelectedIndex = 0;

            cmbStatus.Items.AddRange(new[] { "Playing", "Done", "Dropped", "Wishlist" });
            cmbStatus.SelectedIndex = 0;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введи название игры!");
                return;
            }

            GameTitle = txtTitle.Text;
            Platform = cmbPlatform.SelectedItem?.ToString() ?? "PC";
            Status = cmbStatus.SelectedItem?.ToString() ?? "Playing";

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnOk_Click_1(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}