using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Net.Http;
using Newtonsoft.Json;

namespace CashCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private HttpClient _client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
            txtAmount.TextChanged += txtAmount_TextChanged; //Attaching this event after initialitation because it triggers an error otherwise.
            _client.BaseAddress = new Uri("https://spaghetticode.at/CashCalc/"); 
            //_client.BaseAddress = new Uri("http://localhost/CashCalc/php/");
        }

        private async void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            string input = txtAmount.Text.Replace(',', '.');  // Replace commas with dots
            decimal amount;
            // Try parsing the text to a decimal using invariant culture to ensure period for decimal separator
            if (decimal.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out amount))
            {
                try
                {
                    // Format the decimal amount with a period as decimal separator and make the API call
                    string url = $"cashCalc.php?amount={amount.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                    HttpResponseMessage response = await _client.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status indicates failure
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CashResult>(responseBody);

                    // Update the UI with the results
                    txtTotalPieces.Text = $"Total Pieces Needed: {result.totalPieces}";
                    txtBills.Text = $"Bills: {result.bills}";
                    txtCoins.Text = $"Coins: {result.coins}";
                    txtBreakdown.Text = $"{result.breakdown}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error fetching data: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid numeric amount.");
            }
        }

        public class CashResult
        {
            public int totalPieces { get; set; }
            public int bills { get; set; }
            public int coins { get; set; }
            public string breakdown { get; set; }
        }

        private void txtAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtTotalPieces.Text = "";
            txtBills.Text = "";
            txtCoins.Text = "";
            txtBreakdown.Text = "";
        }
    }
}
