using System.ComponentModel;

public class WordCard : INotifyPropertyChanged
{
    public WordCard()
    {
        EstonianWord = "Tere";
        RussianWord = "Привет";
        IsFlipped = false;
        FrontLabelText = EstonianWord;
    }

    private string estonianWord;
    public string EstonianWord
    {
        get { return estonianWord; }
        set
        {
            if (value != estonianWord)
            {
                estonianWord = value;
                OnPropertyChanged(nameof(EstonianWord));
            }
        }
    }

    private string frontLabelText;
    public string FrontLabelText
    {
        get { return frontLabelText; }
        set
        {
            if (value != frontLabelText)
            {
                frontLabelText = value;
                OnPropertyChanged(nameof(FrontLabelText));
            }
        }
    }

    private string russianWord;
    public string RussianWord
    {
        get { return russianWord; }
        set
        {
            if (value != russianWord)
            {
                russianWord = value;
                OnPropertyChanged(nameof(RussianWord));
            }
        }
    }

    private bool isFlipped;
    public bool IsFlipped
    {
        get { return isFlipped; }
        set
        {
            if (value != isFlipped)
            {
                isFlipped = value;
                OnPropertyChanged(nameof(IsFlipped));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
