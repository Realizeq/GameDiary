using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace GameDiary.Frontend
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _client = new HttpClient();
        private const string ApiUrl = "https://localhost:7064/api/games";

        public Form1()
        {
            InitializeComponent();
            // Игнорируем SSL сертификат (для локальной разработки)
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
            _client = new HttpClient(handler);
        }

        // Загрузка игр при открытии формы
        private async void Form1_Load(object sender, EventArgs e)
        {
            await LoadGames();
        }

        // Получить все игры с API
        private async Task LoadGames()
        {
            try
            {
                var response = await _client.GetStringAsync(ApiUrl);
                var games = JsonSerializer.Deserialize<List<GameDto>>(response,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                dgvGames.DataSource = games;

                // Переименуем колонки на русский
                if (dgvGames.Columns["Id"] != null) dgvGames.Columns["Id"].HeaderText = "ID";
                if (dgvGames.Columns["Title"] != null) dgvGames.Columns["Title"].HeaderText = "Название";
                if (dgvGames.Columns["Platform"] != null) dgvGames.Columns["Platform"].HeaderText = "Платформа";
                if (dgvGames.Columns["Status"] != null) dgvGames.Columns["Status"].HeaderText = "Статус";
                if (dgvGames.Columns["AddedAt"] != null) dgvGames.Columns["AddedAt"].HeaderText = "Добавлена";
                if (dgvGames.Columns["CoverImageUrl"] != null) dgvGames.Columns["CoverImageUrl"].Visible = false;
                if (dgvGames.Columns["Reviews"] != null) dgvGames.Columns["Reviews"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        // Кнопка "Обновить список"
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadGames();
        }

        // Кнопка "Добавить игру"
        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var addForm = new AddGameForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                var game = new
                {
                    title = addForm.GameTitle,
                    platform = addForm.Platform,
                    status = addForm.Status,
                    coverImageUrl = ""
                };

                var json = JsonSerializer.Serialize(game);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _client.PostAsync(ApiUrl, content);
                await LoadGames();
            }
        }

        // Кнопка "Удалить"
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvGames.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выбери игру для удаления!");
                return;
            }

            var id = (int)dgvGames.SelectedRows[0].Cells["Id"].Value;
            var confirm = MessageBox.Show("Удалить игру?", "Подтверждение",
                MessageBoxButtons.YesNo);

            if (confirm == DialogResult.Yes)
            {
                await _client.DeleteAsync(ApiUrl + "/" + id);
                await LoadGames();
            }
        }
    }

    // Класс для чтения данных из API
    public class GameDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Platform { get; set; } = "";
        public string Status { get; set; } = "";
        public string CoverImageUrl { get; set; } = "";
        public DateTime AddedAt { get; set; }
        public List<object> Reviews { get; set; } = new();
    }
}