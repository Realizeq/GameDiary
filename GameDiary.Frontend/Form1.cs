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

                // Добавляем колонку оценки если её нет
                if (!dgvGames.Columns.Contains("Оценка"))
                {
                    var ratingColumn = new DataGridViewTextBoxColumn();
                    ratingColumn.Name = "Оценка";
                    ratingColumn.HeaderText = "Оценка";
                    dgvGames.Columns.Add(ratingColumn);
                }

                // Заполняем оценки
                foreach (DataGridViewRow row in dgvGames.Rows)
                {
                    var gameDto = row.DataBoundItem as GameDto;
                    if (gameDto?.Reviews != null && gameDto.Reviews.Count > 0)
                    {
                        var firstReview = System.Text.Json.JsonSerializer.Deserialize<ReviewDto>(
                            gameDto.Reviews[0].ToString()!,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        row.Cells["Оценка"].Value = firstReview?.Rating + "/10";
                    }
                    else
                    {
                        row.Cells["Оценка"].Value = "—";
                    }
                }
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

                // Добавляем отзыв с оценкой после создания игры

                var json = System.Text.Json.JsonSerializer.Serialize(game);
                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(ApiUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                var createdGame = System.Text.Json.JsonSerializer.Deserialize<GameDto>(responseBody,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (createdGame != null && addForm.Rating > 0)
                {
                    var review = new
                    {
                        gameId = createdGame.Id,
                        rating = addForm.Rating,
                        comment = ""
                    };
                    var reviewJson = System.Text.Json.JsonSerializer.Serialize(review);
                    var reviewContent = new System.Net.Http.StringContent(reviewJson,
                        System.Text.Encoding.UTF8, "application/json");

                    try
                    {
                        var reviewResponse = await _client.PostAsync("https://localhost:7064/api/reviews", reviewContent);
                        var reviewBody = await reviewResponse.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка отзыва: " + ex.Message);
                    }
                }
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



        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvGames.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выбери игру для редактирования!");
                return;
            }

            var id = (int)dgvGames.SelectedRows[0].Cells["Id"].Value;
            var title = dgvGames.SelectedRows[0].Cells["Title"].Value.ToString();
            var platform = dgvGames.SelectedRows[0].Cells["Platform"].Value.ToString();
            var status = dgvGames.SelectedRows[0].Cells["Status"].Value.ToString();

            var editForm = new EditGameForm(id, title, platform, status);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                var updated = new
                {
                    title = editForm.GameTitle,
                    platform = editForm.Platform,
                    status = editForm.Status,
                    coverImageUrl = ""
                };

                var json = System.Text.Json.JsonSerializer.Serialize(updated);
                var content = new System.Net.Http.StringContent(json,
                    System.Text.Encoding.UTF8, "application/json");
                await _client.PutAsync(ApiUrl + "/" + id, content);
                await LoadGames();
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
    public class ReviewDto
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}