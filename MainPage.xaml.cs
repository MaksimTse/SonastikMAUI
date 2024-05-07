
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
                        else
                        {
                            Console.WriteLine($"Skipping line: {line}. Expected format: <EstonianWord> <RussianWord>");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("File does not exist: " + filePath);
                }

                wordCards.Add(new WordCard { IsFlipped = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading word cards: " + ex.Message);
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
                Console.WriteLine("Error during flip animation: " + ex.Message);
            }
        }

        private async void AddWord_Clicked(object sender, EventArgs e)
        {
            string estonianWord = await DisplayPromptAsync("Enter Estonian Word", "Please enter the Estonian word:");
            if (!estonianWord.All(c => "qwertyuiopasdfghjklzxcvbnmüõöäQWERTYUIOPASDFGHJKLZXCVBNMÜÕÖÄ".Contains(c)))
            {
                await DisplayAlert("Error", "Invalid characters in Estonian word.", "OK");
                return;
            }
            string russianWord = await DisplayPromptAsync("Enter Russian Word", "Please enter the Russian word:");
            if (!russianWord.All(c => "йцукенгшщзхъфывапролджэячсмитьбюЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ".Contains(c)))
            {
                await DisplayAlert("Error", "Invalid characters in Russian word.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(estonianWord) || string.IsNullOrWhiteSpace(russianWord))
            {
                await DisplayAlert("Error", "Both Estonian and Russian words are required.", "OK");
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
                await DisplayAlert("Error", "An error occurred while saving the word to file: " + ex.Message, "OK");
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
                        await DisplayAlert("Error", "Word list file does not exist.", "OK");
                        return;
                    }

                    string[] lines = File.ReadAllLines(filePath);

                    string selectedWord = await DisplayActionSheet("Select Word", "Cancel", null, lines);
                    if (selectedWord == "Cancel" || string.IsNullOrWhiteSpace(selectedWord))
                        return;

                    string[] words = selectedWord.Split(' ');
                    if (words.Length != 2)
                    {
                        await DisplayAlert("Error", "Invalid format of word in the list.", "OK");
                        return;
                    }

                    string newEstonianWord = await DisplayPromptAsync("Enter New Estonian Word", "Please enter the new Estonian word:", initialValue: words[0]);
                    if (string.IsNullOrWhiteSpace(newEstonianWord) || !newEstonianWord.All(c => "qwertyuiopasdfghjklzxcvbnmüõöäQWERTYUIOPASDFGHJKLZXCVBNMÜÕÖÄ".Contains(c)))
                    {
                        await DisplayAlert("Error", "There is mistake entering new estonian word.", "OK");
                        return;
                    }

                    string newRussianWord = await DisplayPromptAsync("Enter New Russian Word", "Please enter the new Russian word:", initialValue: words[1]);
                    if (string.IsNullOrWhiteSpace(newRussianWord) || !newRussianWord.All(c => "йцукенгшщзхъфывапролджэячсмитьбюЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ".Contains(c)))
                    {
                        await DisplayAlert("Error", "There is mistake entering new russian word.", "OK");
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
                    await DisplayAlert("Error", "An error occurred while updating the word: " + ex.Message, "OK");
                }
            }
        }



        private async void RemoveWord_Clicked(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "word_cards.txt");
            if (WordCards.Count > 0)
            {
                string[] wordList = File.ReadAllLines(filePath);

                string selectedWord = await DisplayActionSheet("Select Word to Remove", "Cancel", null, wordList);

                if (selectedWord == "Cancel" || string.IsNullOrWhiteSpace(selectedWord))
                    return;

                string[] words = selectedWord.Split(new string[] { " - " }, StringSplitOptions.None);

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
                        await DisplayAlert("Error", "An error occurred while removing the word from file: " + ex.Message, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Selected word not found.", "OK");
                }
            }
        }
    }
}