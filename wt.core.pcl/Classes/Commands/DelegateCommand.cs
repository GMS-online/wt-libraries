﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Logging;

namespace WhileTrue.Classes.Commands
{
    ///<summary>
    /// Provides a class to implement ICommand interface with the use of delegates
    ///</summary>
    /// <remarks>
    /// <para>
    /// This class exists in two versions. One of it is generic.
    /// If using the generic version, the type given represents the type of the parameter
    /// expected, which will be automatically casted prior to the call of the delegates.
    /// </para>
    /// <para>
    /// Calls to the delegates are dispatched into the thread the delegatecommand was created in.
    /// </para>
    /// </remarks>
    public class DelegateCommand<TParameterType> : ICommand
    {
        private readonly Action<TParameterType> executeDelegate;
        private readonly string name;
        private readonly Action<Exception> exceptionHandler;
        readonly NotifyChangeExpression<Func<TParameterType, bool>> canExecuteDelegateExpression;
        private readonly Func<TParameterType, bool> canExecuteDelegate;
        // ReSharper disable once NotAccessedField.Local - see comment at usage below
        private EventHandler requerySuggestedEventHandler;
        private readonly ReaderWriterLockSlim executeLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Implements a command which is always executable
        /// </summary>
        public DelegateCommand(Action<TParameterType> executeDelegate, string name = null, Action<Exception> exceptionHandler=null )
            :this(executeDelegate, _ => true, name, exceptionHandler)
        {

        }

        /// <summary>
        /// Implements a command which executable state is retrieved using the second delegate and supports automatic 
        /// updation of the executable state
        /// </summary>
        /// <remarks>
        /// <para>
        /// The canExecuteDelegate is automatically parsed for changes of the properties called
        /// (providing the objects which are used implement <see cref="INotifyPropertyChanged"/> and/or 
        /// <see cref="INotifyCollectionChanged"/>)
        /// </para>
        /// <para>
        /// This is most useful for commands which executable state changes asynchronuously which is not
        /// detected using the default WPF executable state polling.
        /// </para>
        /// </remarks>
        public DelegateCommand(Action<TParameterType> executeDelegate, Expression<Func<TParameterType, bool>> canExecuteExpression, string name=null, Action<Exception> exceptionHandler = null)
        {
            this.canExecuteDelegateExpression = new NotifyChangeExpression<Func<TParameterType, bool>>(canExecuteExpression);
            this.executeDelegate = executeDelegate;
            this.name = name;
            this.exceptionHandler = exceptionHandler;
            this.canExecuteDelegate = this.canExecuteDelegateExpression.Invoke;

            this.AttachRequerySuggestedEvent();
            this.canExecuteDelegateExpression.Changed += (_, __) => this.InvokeCanExecuteChanged();
        }

        private void AttachRequerySuggestedEvent()
        {
            // We have to save the handler in this class, as the CommandManager.RequerySuggested event
            // is realized as a list of weak references to the event handlers, so it would be removed
            // immediately if it is not saved here.
            this.requerySuggestedEventHandler = delegate { this.NotifyRequerySuggested(); };
        }

        private void NotifyRequerySuggested()
        {
            this.CanExecuteChanged(this, EventArgs.Empty); 
        }

        /// <summary>
        /// Fires the CanExecuteChanged event on the command
        /// </summary>
        private void InvokeCanExecuteChanged()
        {
            DebugLogger.WriteLine(this, LoggingLevel.Normal, () => $"DelegateCommand '{this.name ?? "<unset>"}' CanExecuteChanged fired.");
            DebugLogger.WriteLine(this, LoggingLevel.Verbose, () =>
            {
                bool? CanExecuteValue;
                try
                {
                    CanExecuteValue = this.CanExecute(null);
                }
                catch
                {
                    CanExecuteValue = null;
                }
                return $"DelegateCommand '{this.name ?? "<unset>"}' New value for CanExecute (with <null> param): '{(CanExecuteValue.HasValue ? CanExecuteValue.Value.ToString() : "<exception occurred>")}'";
            });
            this.CanExecuteChanged(this, EventArgs.Empty);
        }


        /// <summary>
        /// <see cref="ICommand.CanExecuteChanged"/>
        /// </summary>
        public event EventHandler CanExecuteChanged = delegate{};

        /// <summary>
        /// <see cref="ICommand.Execute"/>
        /// </summary>
        public async void Execute(object parameter)
        {
            if (this.executeLock.TryEnterWriteLock(0))
            {
                try
                {
                    Task Result = this.executeDelegate.DynamicInvoke((TParameterType) (parameter ?? default(TParameterType))) as Task;
                    if (Result != null)
                    {
                        //Delegate is asynchronous, await its completion. This is done to release lock only if the delegate was executed completly to avoid re-entries
                        await Result;
                    }
                    else
                    {
                        //Delegate is synchronous, just continue
                    }
                }
                catch (Exception Exception)
                {
                    if (this.exceptionHandler != null)
                    {
                        this.exceptionHandler(Exception);
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    this.executeLock.ExitWriteLock();
                }
            }
            else
            {
                //Ignore execution, last execution command is not completed yet
            }
        }

        
        /// <summary>
        /// <see cref="ICommand.CanExecute"/>
        /// </summary>
        public bool CanExecute(object parameter)
        {
            try
            {
                bool CanExecute = this.canExecuteDelegate((TParameterType) (parameter ?? default(TParameterType)));
                DebugLogger.WriteLine(this, LoggingLevel.Verbose, () => $"DelegateCommand '{this.name ?? "<unset>"}' Value queried for CanExecute (with '{parameter}' param): '{CanExecute}'");
                return CanExecute;
            }
            catch (Exception Exception)
            {
                if (this.exceptionHandler != null)
                {
                    this.exceptionHandler(Exception);
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
    }

    ///<summary>
    /// Provides a class to implement ICommand interface with the use of delegates
    ///</summary>
    /// <remarks>
    /// This class exists in two versions. One of it is generic.
    /// If using the generic version, the type given represents the type of the parameter
    /// expected, which will be automatically casted prior to the call of the delegates.
    /// </remarks>
    public class DelegateCommand : DelegateCommand<object>
    {
        /// <summary>
        /// Implements a command which is always executable
        /// </summary>
        public DelegateCommand(Action executeDelegate, string name=null, Action<Exception> exceptionHandler = null)
            : base(_ => executeDelegate(), name, exceptionHandler)
        {
        }

        /// <summary>
        /// Implements a command which executable state is retrieved using the second delegate and supports automatic 
        /// updation of the executable state
        /// </summary>
        /// <remarks>
        /// <para>
        /// The canExecuteDelegate is automatically parsed for changes of the properties called
        /// (providing the objects which are used implement <see cref="INotifyPropertyChanged"/> and/or 
        /// <see cref="INotifyCollectionChanged"/>)
        /// </para>
        /// <para>
        /// This is most useful for commands which executable state changes asynchronuously which is not
        /// detected using the default WPF executable state polling.
        /// </para>
        /// </remarks>
        public DelegateCommand(Action executeDelegate, Expression<Func<bool>> canExecuteExpression, string name=null, Action<Exception> exceptionHandler = null)
            : base(_ => executeDelegate(), (Expression<Func<object, bool>>)Expression.Lambda(canExecuteExpression.Body, Expression.Parameter(typeof(object), "_")), name, exceptionHandler)
        {
        }
    }
}
