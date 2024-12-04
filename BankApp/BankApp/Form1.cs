using Newtonsoft.Json;
using System.Net.Http.Json;

namespace BankApp
{
    public partial class Form1 : Form
    {
        public string token;
        public Form1()
        {
            InitializeComponent();
        }


        private void GetAccountData(object sender, EventArgs e)
        {
            //to jest blibioteka do wysy�ania zapyta� http
            //i przetwarzania odpowiedzi otrzymanych z API
            HttpClient client = new HttpClient();
            //adres API - endpoint zwraca szczeg�y rachunku na podstawie tokenu
            string url = "http://localhost/bankAPI/account/details/";
            //tworzymy obiekt zawieraj�cy token
            var data = new { token = token };
            //wysy�amy zapytanie POST do API zawieraj�ce token
            HttpResponseMessage response = client.PostAsJsonAsync(url, data).Result;
            //wyci�gnij z odpowiedzi dane w formacie JSON
            string json = response.Content.ReadAsStringAsync().Result;
            Account account = JsonConvert.DeserializeObject<Account>(json);
            //wypisz dane na formularzu
            AccountNameTextBox.Text = account.name;
            AccountNumberTextBox.Text = account.accountNo.ToString();
            AccountAmountTextBox.Text = account.amount.ToString();

        }

        private void OnAppLoad(object sender, EventArgs e)
        {
            Login loginForm = new Login(this);
            if (loginForm.ShowDialog(this) == DialogResult.OK)
            {
                //je�li zalogowano poprawnie to poka� formularz
                this.Show();
                tokenTextBox.Text = token;
            }
            else
            {
                //je�li nie to zamknij aplikacj�
                Application.Exit();
            }
        }

        private void newTransferButton_Click(object sender, EventArgs e)
        {
            //otw�rz formularz nowego przelewu
            NewTransfer newTransfer = new NewTransfer();

            newTransfer.token = token;
            newTransfer.source = AccountNumberTextBox.Text;

            newTransfer.ShowDialog();
            //TODO: poka� zaktualizowany stan konta po wykonaniu przelewu
        }

        private async void buttontransfery_Click(object sender, EventArgs e)
        {
            listViewTransfers.Items.Clear();

            try
            {
                // Wywo�ujemy API, aby pobra� przelewy
                var transfers = await GetTransfersAsync(token);

                // Sprawdzamy, czy transfers nie jest null
                if (transfers != null)
                {
                    // Dodajemy ka�dy transfer do listy
                    foreach (var transfer in transfers)
                    {
                        var item = new ListViewItem(transfer.created_at.ToString());
                        item.SubItems.Add(transfer.source_account.ToString());
                        item.SubItems.Add(transfer.target_account.ToString());
                        item.SubItems.Add(transfer.amount.ToString());
                        listViewTransfers.Items.Add(item);
                    }
                }
                else
                {
                    MessageBox.Show("Nie uda�o si� pobra� przelew�w.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"B��d przy pobieraniu przelew�w: {ex.Message}");
            }
        }


        private async Task<dynamic[]> GetTransfersAsync(string token)
        {
            using (var client = new HttpClient())
            {
                string url = "http://localhost/bankAPI/transfers";

                var requestData = new
                {
                    token = token
                };

                var response = await client.PostAsync(
                    url,
                    new StringContent(JsonConvert.SerializeObject(requestData), System.Text.Encoding.UTF8, "application/json")
                );

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                MessageBox.Show(responseBody);  // Debugowanie odpowiedzi serwera

                return JsonConvert.DeserializeObject<dynamic[]>(responseBody);
            }
        }
    }
}

