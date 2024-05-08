
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;

namespace SonastikMAUI
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<WordCard> WordCards { get; set; }
        public Command FlipCommand { get; }

        public MainPage()
        {
            InitializeComponent();

            WordCards = LoadWordCardsFromFile();
            BindingContext = this;

        }
        private void UpdateWordCards()
        {
            WordCards.Clear();
            ObservableCollection<WordCard> updatedWordCards = LoadWordCardsFromFile();
            foreach (var wordCard in updatedWordCards)
            {
                WordCards.Add(wordCard);
            }
        }

        private ObservableCollection<WordCard> LoadWordCardsFromFile()
        {
            ObservableCollection<WordCard> wordCards = new ObservableCollection<WordCard>();

            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "word_cards.txt");

                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    foreach (string line in lines)
                    {
                        string[] words = line.Split(' ');
                        if (words.Length == 2)
                        {
                            WordCard card = new WordCard { EstonianWord = words[0], RussianWord = words[1], IsFlipped = false };
                            card.FrontLabelText = card.EstonianWord;
                            wordCards.Add(card);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Faili ei ole olemas: " + filePath);
                }

                wordCards.Add(new WordCard { IsFlipped = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Viga sõnakaartide laadimisel: " + ex.Message);
            }

            return wordCards;
        }



        private async void FlipButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Frame frame && frame.BindingContext is WordCard wordCard)
            {
                Label frontLabel = frame.FindByName<Label>("FrontLabel");
                Label backLabel = frame.FindByName<Label>("BackLabel");

                if (!wordCard.IsFlipped)
                {
                    frontLabel.IsVisible = true;
                    backLabel.IsVisible = false;
                }
                else
                {
                    frontLabel.IsVisible = false;
                    backLabel.IsVisible = true;
                }



                await FlipCardAnimation(frame);

                frontLabel.Text = wordCard.EstonianWord;

                wordCard.IsFlipped = !wordCard.IsFlipped;
            }
        }


        private async Task FlipCardAnimation(Frame cardFrame)
        {
            try
            {
                await cardFrame.RotateYTo(-115, 250, Easing.Linear);

                await Task.Delay(50);

                await cardFrame.RotateYTo(0, 250, Easing.Linear);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Viga pööramisanimatsiooni ajal: " + ex.Message);
            }
        }

        private async void AddWord_Clicked(object sender, EventArgs e)
        {
            string estonianWord = await DisplayPromptAsync("Sisesta eesti sõna", "Palun sisesta eestikeelne sõna:");
            if (string.IsNullOrWhiteSpace(estonianWord) || !estonianWord.All(c => "qwertyuiopasdfghjklzxcvbnmüõöäQWERTYUIOPASDFGHJKLZXCVBNMÜÕÖÄ1234567890".Contains(c)))
            {
                await DisplayAlert("Viga", "Eestikeelses sõnas vigased märgid.", "OK");
                return;
            }
            string russianWord = await DisplayPromptAsync("Sisestage vene sõna", "Palun sisestage venekeelne sõna:");
            if (string.IsNullOrWhiteSpace(russianWord) || !russianWord.All(c => "йцукенгшщзхъфывапролджэячсмитьбюЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ1234567890".Contains(c)))
            {
                await DisplayAlert("Viga", "Venekeelses sõnas on kehtetud tähemärgid.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(estonianWord) || string.IsNullOrWhiteSpace(russianWord))
            {
                await DisplayAlert("Viga", "Nõutavad on nii eesti kui ka vene sõnad.", "OK");
                return;
            }
            WordCard newWordCard = new WordCard { EstonianWord = estonianWord, RussianWord = russianWord, IsFlipped = false };
            WordCards.Add(newWordCard);

            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "word_cards.txt");
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.WriteLine($"{estonianWord} {russianWord}");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Viga", "Sõna faili salvestamisel ilmnes viga: " + ex.Message, "OK");
            }
        }

        private async void ChangeWord_Clicked(object sender, EventArgs e)
        {
            if (WordCards.Count > 0)
            {
                var wordCard = WordCards[WordCards.Count - 1];

                try
                {
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "word_cards.txt");
                    if (!File.Exists(filePath))
                    {
                        await DisplayAlert("Viga", "Sõnade loendi faili pole olemas.", "OK");
                        return;
                    }

                    string[] lines = File.ReadAllLines(filePath);

                    string selectedWord = await DisplayActionSheet("Valige Sõna", "Loobu", null, lines);
                    if (selectedWord == "Loobu" || string.IsNullOrWhiteSpace(selectedWord))
                        return;

                    string[] words = selectedWord.Split(' ');
                    if (words.Length != 2)
                    {
                        await DisplayAlert("Viga", "Loendis oleva sõna vorming on vale.", "OK");
                        return;
                    }

                    string newEstonianWord = await DisplayPromptAsync("Sisestage uus eesti sõna", "Palun sisestage uus eestikeelne sõna:", initialValue: words[0]);
                    if (string.IsNullOrWhiteSpace(newEstonianWord) || !newEstonianWord.All(c => "qwertyuiopasdfghjklzxcvbnmüõöäQWERTYUIOPASDFGHJKLZXCVBNMÜÕÖÄ1234567890".Contains(c)))
                    {
                        await DisplayAlert("Viga", "Uue eestikeelse sõna sisestamisel on viga.", "OK");
                        return;
                    }

                    string newRussianWord = await DisplayPromptAsync("Sisestage uus vene sõna", "Palun sisestage uus venekeelne sõna:", initialValue: words[1]);
                    if (string.IsNullOrWhiteSpace(newRussianWord) || !newRussianWord.All(c => "йцукенгшщзхъфывапролджэячсмитьбюЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ1234567890".Contains(c)))
                    {
                        await DisplayAlert("Viga", "Uue venekeelse sõna sisestamisel on viga.", "OK");
                        ChangeWord_Clicked(sender, e);
                        return;
                    }

                    wordCard.EstonianWord = newEstonianWord;
                    wordCard.RussianWord = newRussianWord;

                    string oldLine = $"{words[0]} {words[1]}";
                    string newLine = $"{newEstonianWord} {newRussianWord}";
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i] == oldLine)
                        {
                            lines[i] = newLine;
                            break;
                        }
                    }

                    File.WriteAllLines(filePath, lines);

                    UpdateWordCards();

                    if (!lines.Contains(newLine))
                    {
                        using (StreamWriter writer = File.AppendText(filePath))
                        {
                            writer.WriteLine(newLine);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Viga", "Sõna värskendamisel ilmnes viga: " + ex.Message, "OK");
                }
            }
        }

        private async void RemoveWord_Clicked(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "word_cards.txt");
            if (WordCards.Count > 0)
            {
                string[] wordList = File.ReadAllLines(filePath);

                string selectedWord = await DisplayActionSheet("Valige eemaldatav Sõna", "Loobu", null, wordList);

                if (selectedWord == "Loobu" || string.IsNullOrWhiteSpace(selectedWord))
                    return;

                string[] words = selectedWord.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                WordCard removedWord = WordCards.FirstOrDefault(card => card.EstonianWord == words[0] && card.RussianWord == words[1]);

                if (removedWord != null)
                {
                    WordCards.Remove(removedWord);

                    try
                    {
                        string[] lines = File.ReadAllLines(filePath);
                        string removedLine = $"{removedWord.EstonianWord} {removedWord.RussianWord}";

                        lines = lines.Where(line => line.Trim() != removedLine.Trim()).ToArray();
                        File.WriteAllLines(filePath, lines);
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Viga", "Sõna failist eemaldamisel ilmnes viga: " + ex.Message, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Viga", "Valitud sõna ei leitud.", "OK");
                }
            }
        }
    }
}