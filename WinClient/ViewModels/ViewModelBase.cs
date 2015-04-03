using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Relational.Octapus.Common;
using System.ComponentModel;

namespace Relational.Octapus.WinClient.ViewModels
{
    public abstract class ViewModelBase : NotifyPropertyChanged, IDisposable
    {
        protected bool isDirty;
        protected bool isBusy;
        protected string displayName, executableBuildNumber;

        protected static string PropertyOf<T>(Expression<Func<T, object>> propertySelector)
        {
            return Reflect<T>.PropertyName(propertySelector);
        }

        public bool IsDirty
        {
            get { return isDirty; }
            protected set
            {
                if (isDirty == value) return;
                isDirty = value;
                NotifyChanged(() => IsDirty);
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy == value) return;
                isBusy = value;
                NotifyChanged(() => IsBusy);
            }
        }

        #region DisplayName

        /// <summary>
        /// Returns the user-friendly name of this object.
        /// Child classes can set this property to a new value
        /// or override it to determine the value on-demand.
        /// </summary>
        public virtual string DisplayName
        {
            get { return displayName; }
            set
            {
                if (displayName == value) return;
                displayName = value;
                NotifyChanged(() => DisplayName);
            }
        }

        public string ExecutableBuildNumber
        {
            get
            {
                return this.executableBuildNumber;
            }
            set
            {
                string oldValue = executableBuildNumber;
                executableBuildNumber = value;
                NotifyChanged("ExecutableBuildNumber", oldValue, value);

                //this.text = value;
                //if (null != PropertyChanged)
                //{
                //    this.PropertyChanged(this, new PropertyChangedEventArgs("ExecutableBuildNumber"));
                //}
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion // DisplayName

        #region IDisposable Members

        /// <summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            OnDispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose(bool disposing)
        {
        }


        #endregion // IDisposable Members
    }
}
