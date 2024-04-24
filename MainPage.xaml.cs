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

            FlipCommand = new Command(ExecuteFlipCommand);

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "word_cards.txt");
            if (!File.Exists(filePath))
            {
                CreateInitialWordCardsFile(filePath);
            }

            WordCards = LoadWordCardsFromFile(filePath);
            BindingContext = this;
        }

        private ObservableCollection<WordCard> LoadWordCardsFromFile(string filePath)
        {
            ObservableCollection<WordCard> wordCards = new ObservableCollection<WordCard>();

            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading word cards: " + ex.Message);
            }

            return wordCards;
        }

        private void CreateInitialWordCardsFile(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Tere Привет");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating initial word cards file: " + ex.Message);
            }
        }
        private async Task FlipCardAnimation(Frame cardFrame, Label frontLabel, Label backLabel, bool isFront)
        {
            await cardFrame.RotateYTo(-115, 250, Easing.Linear);

            if (isFront)
            {
                frontLabel.IsVisible = false;
                backLabel.IsVisible = true;
            }
            else
            {
                frontLabel.IsVisible = true;
                backLabel.IsVisible = false;
            }

            await cardFrame.RotateYTo(0, 250, Easing.Linear);
        }
        private async void ExecuteFlipCommand(object obj)
        {
            if (obj is Frame frame && frame.BindingContext is WordCard wordCard)
            {
                Label frontLabel = frame.FindByName<Label>("FrontLabel");
                Label backLabel = frame.FindByName<Label>("BackLabel");

                // Animate the flip
                await FlipCardAnimation(frame, frontLabel, backLabel, wordCard.IsFlipped);

                // After the flip animation completes, update the text of the labels
                if (wordCard.IsFlipped)
                {
                    frontLabel.IsVisible = false;
                    backLabel.IsVisible = true;
                    frontLabel.Text = wordCard.RussianWord;
                    backLabel.Text = wordCard.EstonianWord;
                }
                else
                {
                    frontLabel.IsVisible = true;
                    backLabel.IsVisible = false;
                    frontLabel.Text = wordCard.EstonianWord;
                    backLabel.Text = wordCard.RussianWord;
                }

                // Toggle the IsFlipped property
                wordCard.IsFlipped = !wordCard.IsFlipped;
            }
        }

        private void AddWord_Clicked(object sender, EventArgs e)
        {
            WordCards.Add(new WordCard { EstonianWord = "New Estonian Word", RussianWord = "New Russian Word", IsFlipped = false });
        }

        private void RemoveWord_Clicked(object sender, EventArgs e)
        {
            if (WordCards.Any())
                WordCards.RemoveAt(WordCards.Count - 1);
        }
        private void ChangeWord_Clicked(object sender, EventArgs e)
        {
            if (WordCards.Any())
            {
                var wordCard = WordCards.Last();
                wordCard.EstonianWord = "Changed Estonian Word";
                wordCard.RussianWord = "Changed Russian Word";
            }
        }
    }
}