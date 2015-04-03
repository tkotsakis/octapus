using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Relational.Octapus.WinClient
{
    public abstract class BaseCommand : NotifyPropertyChanged, ICommand
    {
        string displayText;
        string displayIcon;
        string description;

        public BaseCommand()
        {
        }

        public BaseCommand(string displayText)
        {
            this.displayText = string.IsNullOrWhiteSpace(displayText) ? GetType().Name : displayText;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public string DisplayText
        {
            get { return displayText; }
            set
            {
                if (displayText != value)
                {
                    displayText = value;
                    NotifyChanged(() => DisplayText);
                }
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                    NotifyChanged(() => Description);
                }
            }
        }

        public string DisplayIcon
        {
            get { return displayIcon; }
            set
            {
                if (displayIcon != value)
                {
                    displayIcon = value;
                    NotifyChanged(() => DisplayIcon);
                }
            }
        }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute()
        {
            Execute(null);
        }

        public abstract void Execute(object parameter);

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(displayText))
                return base.ToString();
            return displayText;
        }
    }
}
