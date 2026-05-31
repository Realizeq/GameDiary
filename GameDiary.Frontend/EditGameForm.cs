namespace GameDiary.Frontend
{
    public partial class EditGameForm : Form
    {
        public string GameTitle { get; private set; } = "";
        public string Platform { get; private set; } = "";
        public string Status { get; private set; } = "";

        public EditGameForm(int id, string title, string platform, string status)
        {
            InitializeComponent();

            cmbPlatform.Items.AddRange(new[] { "PC", "PS5", "PS4", "Xbox", "Nintendo Switch" });
            cmbStatus.Items.AddRange(new[] { "В процессе", "Пройдена", "Брошена", "Вишлист" });

            txtTitle.Text = title;
            cmbPlatform.SelectedItem = platform;
            cmbStatus.SelectedItem = status;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введи название игры!");
                return;
            }

            GameTitle = txtTitle.Text;
            Platform = cmbPlatform.SelectedItem?.ToString() ?? "PC";
            Status = cmbStatus.SelectedItem?.ToString() ?? "В процессе";

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}