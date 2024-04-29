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

            // Load word cards from file
            WordCards = LoadWordCardsFromFile();
            BindingContext = this;

        }

        // Load word cards from file
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
                            wordCards.Add(new WordCard { EstonianWord = words[0], RussianWord = words[1], IsFlipped = false });
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

                // Добавляем карточку без указания слов, чтобы установить текст по умолчанию
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

                // Переключаем значения видимости меток и текста в зависимости от того, перевернута ли карточка
                if (!wordCard.IsFlipped)
                {
                    frontLabel.IsVisible = false;
                    backLabel.IsVisible = true;
                    frontLabel.Text = wordCard.EstonianWord;
                    backLabel.Text = wordCard.RussianWord;
                }
                else
                {
                    frontLabel.IsVisible = true;
                    backLabel.IsVisible = false;
                    frontLabel.Text = wordCard.EstonianWord; // Устанавливаем эстонское слово в метку FrontLabel
                    backLabel.Text = wordCard.RussianWord;
                }

                wordCard.IsFlipped = !wordCard.IsFlipped;
            }
        }



        private async Task FlipCardAnimation(Frame cardFrame)
        {
            try
            {
                await cardFrame.RotateYTo(-115, 250, Easing.Linear);

                await Task.Delay(100); // Delay for better animation effect

                await cardFrame.RotateYTo(0, 250, Easing.Linear);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during flip animation: " + ex.Message);
            }
        }

        // Add a new word card with custom Estonian and Russian words
        private async void AddWord_Clicked(object sender, EventArgs e)
        {
            // Предложить пользователю ввести эстонское и русское слова
            string estonianWord = await DisplayPromptAsync("Enter Estonian Word", "Please enter the Estonian word:");
            string russianWord = await DisplayPromptAsync("Enter Russian Word", "Please enter the Russian word:");

            // Проверить, что оба слова были введены
            if (string.IsNullOrWhiteSpace(estonianWord) || string.IsNullOrWhiteSpace(russianWord))
            {
                await DisplayAlert("Error", "Both Estonian and Russian words are required.", "OK");
                return;
            }

            // Добавить новую карточку со словами
            WordCards.Add(new WordCard { EstonianWord = estonianWord, RussianWord = russianWord, IsFlipped = false });
        }
        // Change the last word card by selecting from a list of words loaded from a text file
        // Change the last word card by selecting from a list of words loaded from a text file
        private async void ChangeWord_Clicked(object sender, EventArgs e)
        {
            if (WordCards.Count > 0)
            {
                // Get the last added word card
                var wordCard = WordCards[WordCards.Count - 1];

                try
                {
                    // Load words from the text file
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "word_cards.txt");
                    if (!File.Exists(filePath))
                    {
                        await DisplayAlert("Error", "Word list file does not exist.", "OK");
                        return;
                    }

                    // Read all lines from the file
                    string[] lines = File.ReadAllLines(filePath);

                    // Show the list of words to the user and let them choose
                    string selectedWord = await DisplayActionSheet("Select Word", "Cancel", null, lines);
                    if (selectedWord == "Cancel" || string.IsNullOrWhiteSpace(selectedWord))
                        return;

                    // Split the selected word into Estonian and Russian words
                    string[] words = selectedWord.Split(' ');
                    if (words.Length != 2)
                    {
                        await DisplayAlert("Error", "Invalid format of word in the list.", "OK");
                        return;
                    }

                    // Prompt the user to enter new words for replacement
                    string newEstonianWord = await DisplayPromptAsync("Enter New Estonian Word", "Please enter the new Estonian word:", initialValue: words[0]);
                    string newRussianWord = await DisplayPromptAsync("Enter New Russian Word", "Please enter the new Russian word:", initialValue: words[1]);

                    // Check if both new words are entered
                    if (string.IsNullOrWhiteSpace(newEstonianWord) || string.IsNullOrWhiteSpace(newRussianWord))
                    {
                        await DisplayAlert("Error", "Both Estonian and Russian words are required.", "OK");
                        return;
                    }

                    // Update the word card with the new words
                    wordCard.EstonianWord = newEstonianWord;
                    wordCard.RussianWord = newRussianWord;
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", "An error occurred while loading words: " + ex.Message, "OK");
                }
            }
        }



        // Remove the last word card
        private void RemoveWord_Clicked(object sender, EventArgs e)
        {
            if (WordCards.Count > 0)
                WordCards.RemoveAt(WordCards.Count - 1);
        }
    }
}