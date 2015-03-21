using System;
using System.Windows.Input;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    public class AtrHistoricalCharactersTokenAdapter : AtrTokenAdapterBase
    {
        private readonly AtrHistoricalCharactersToken historicalCharactersToken;
        private string characters;
        private string error;
        private readonly DelegateCommand clearHistoricalCharactersCommand;

        public AtrHistoricalCharactersTokenAdapter(AtrHistoricalCharactersToken historicalCharactersToken) : base(historicalCharactersToken)
        {
            this.historicalCharactersToken = historicalCharactersToken;
            this.historicalCharactersToken.PropertyChanged += historicalCharactersToken_PropertyChanged;
            this.characters = this.historicalCharactersToken.HistoricalCharacters.ToHexString(" ");

            this.clearHistoricalCharactersCommand = new DelegateCommand(this.ClearHistoricalCharacters,()=>this.historicalCharactersToken.HistoricalCharacters.Length!=0);

            this.AddValidationForProperty(() => Characters)
                .AddValidation(_ => this.Error == null, _ => this.Error);
        }

        void historicalCharactersToken_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ReflectionHelper.GetPropertyName(() => ((AtrHistoricalCharactersToken) sender).HistoricalCharacters))
            {
                this.characters = this.historicalCharactersToken.HistoricalCharacters.ToHexString(" ");
                this.InvokePropertyChanged(()=>Characters);
            }
        }

        private void ClearHistoricalCharacters()
        {
            this.Characters = "";
        }

        public string Characters
        {
            get { return this.characters; }
            set
            {
                this.SetAndInvoke(() => Characters, ref this.characters, value);
                try
                {
                    byte[] CharactersBytes = this.characters.ToByteArray();
                    if (CharactersBytes.Length <= 15)
                    {
                        this.Error = null;
                        if (this.historicalCharactersToken.HistoricalCharacters.HasEqualValue(CharactersBytes) == false)
                        {
//avoid recursion
                            try
                            {
                                this.historicalCharactersToken.HistoricalCharacters = CharactersBytes;
                                string CharactersValue = CharactersBytes.ToHexString(" ");
                                if (this.characters.Trim() != CharactersValue)
                                {
                                    //reformat if value is different
                                    this.characters = CharactersValue;
                                }
                            }
                            catch (Exception Exception)
                            {
                                this.Error = Exception.Message;
                            }
                        }
                    }
                    else
                    {
                        this.characters = CharactersBytes.ToHexString(" ");
                        this.Error = "must be less or equal than 15 bytes";
                    }
                }
                catch (Exception Exception)
                {
                    this.Error = "not a hexadecimal value";
                }
            }
        }

        public string Error
        {
            get { return this.error; }
            set { this.SetAndInvoke(() => Error, ref this.error, value); }
        }

        public ICommand ClearHistoricalCharactersCommand { get { return this.clearHistoricalCharactersCommand; } }

    }
}