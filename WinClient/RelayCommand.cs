using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.WinClient
{
    public class RelayCommand : BaseCommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            : this(null, execute, canExecute)
        {
        }

        public RelayCommand(string displayText, Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
            DisplayText = displayText;
            DisplayIcon = string.Empty;
        }

        public override bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
