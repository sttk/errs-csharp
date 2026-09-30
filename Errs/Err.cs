/*
 * Err.cs
 * Copyright (C) 2026 Takayuki Sato. All Rights Reserved.
 */

namespace Errs;

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Is the exception class with a reason.<br/>
/// <br/>
/// This class has a property which indicates a reason for this exception. Typically the type of
/// this property is a record class. In this case, the class name of this record represents
/// the reason, and the fields of the record hold the situation where the exception occurred.<br/>
/// <br/>
/// Optionally, this exception class can notify its instance creation to pre-registered exception
/// handlers. This notification feature can be enabled by specifying the command line argument
/// {@code --github.sttk.errs.notify=true} when an application using this library is started.<br/>
/// <br/>
/// <example>
/// The example code of creating and throwing an exception is as follows:
/// <code>
/// public record FailToDoSomething(String name, int value);
///
/// try
/// {
///     throw new Err(new FailToDoSomething("abc", 123));
/// }
/// catch (Err e)
/// {
///     Console.WriteLine(e.Message);
/// }
/// </code>
/// </example>
/// </summary>
public sealed class Err : Exception
{
    /// <summary>
    /// The reason for this exception.
    /// </summary>
    /// <value>
    /// The reason for this exception.
    /// </value>
    public object Reason { get; }

    /// <summary>
    /// The name of the source file of this exception occurrence.
    /// </summary>
    /// <value>
    /// The name of the source file of this exception occurrence.
    /// </value>
    public string File { get; }

    /// <summary>
    /// The line number of this exception occurrence in the source file.
    /// </summary>
    /// <value>
    /// The line number of this exception occurrence in the source file.
    /// </value>
    public int Line { get; }

    /// <summary>
    /// Is the constructor which takes an object indicating the reason for this exception.
    /// In addition, this constructor takes the inner exception indicating the cause for this
    /// exception if any.
    /// </summary>
    /// <param name="reason">
    /// An object representing the reason for the exception. The type of this object identifies
    /// the kind of error and may contain additional error details.
    /// </param>
    /// <param name="innerException">
    /// The underlying exception that caused the error, if any.
    /// </param>
    /// <param name="file">
    /// The source file in which the error was created.
    /// This parameter is automatically populated by the compiler.
    /// </param>
    /// <param name="line">
    /// The line number at which the error was created.
    /// This parameter is automatically populated by the compiler.
    /// </param>
    public Err(
      object reason,
      Exception? innerException = null,
      [CallerFilePath] string file = "",
      [CallerLineNumber] int line = 0)
      : base(null, innerException)
    {
        ArgumentNullException.ThrowIfNull(reason);

        Reason = reason;
        File = Path.GetFileName(file);
        Line = line;

        notifyErr(this);
    }

    /// <inheritdoc/>
    public override string Message => Reason.ToString() ?? "";

    /// <inheritdoc/>
    public override string ToString()
    {
        var str = GetType().FullName;
        str += " { reason = " + Reason.GetType().FullName + " " + Reason.ToString();
        str += ", file = " + File + ", line = " + Line;
        if (InnerException != null)
        {
            str += ", cause = " + InnerException.ToString();
        }
        str += " }";

        return str;
    }

    //// Notification ////

#if DEBUG   
    internal static bool _useNotification;
    internal static bool _isHandlersFixed = false;
    internal static readonly List<SyncErrHandler> _syncErrHandlers = new();
    internal static readonly List<AsyncErrHandler> _asyncErrHandlers = new();
#else
    private static readonly bool _useNotification;

    static Err()
    {
        bool b = false;
        foreach (var arg in Environment.GetCommandLineArgs())
        {
            if (arg == "--github.sttk.errs.notify=true")
            {
                b = true;
                break;
            }
        }
        _useNotification = b;
    }

    private static bool _isHandlersFixed = false;
    private static readonly List<SyncErrHandler> _syncErrHandlers = new();
    private static readonly List<AsyncErrHandler> _asyncErrHandlers = new();
#endif

    /// <summary>
    /// Adds an <see cref="SyncErrHandler"/> object which is executed synchronously just after an
    /// <see cref="Err"/> is created. Handlers added with this method are executed in the order
    /// of addition and stop if one of the handlers throws an exception.
    /// NOTE: This feature is enabled via the command line argument:
    /// <c>--github.sttk.errs.notify=true</c>
    /// </summary>
    /// <param name="handler">
    /// An <see cref="SyncErrHandler"/> object.
    /// </param>
    public static void AddSyncHandler(SyncErrHandler handler)
    {
        if (!_useNotification) return;
        if (_isHandlersFixed) return;
        _syncErrHandlers.Add(handler);
    }

    /// <summary>
    /// Adds an <see cref="AsyncErrHandler"/> object which is executed asynchronously just after
    /// an <see cref="Err"/> is created. Handlers don't stop even if one of the handlers throw an
    /// exception.
    /// NOTE: This feature is enabled via the command line argument:
    /// <c>--github.sttk.errs.notify=true</c>
    /// </summary>
    /// <param name="handler">
    /// An <see cref="AsyncErrHandler"/> object.
    /// </param>
    public static void AddAsyncHandler(AsyncErrHandler handler)
    {
        if (!_useNotification) return;
        if (_isHandlersFixed) return;
        _asyncErrHandlers.Add(handler);
    }

    /// <summary>
    /// Prevents further addition of error handler object to synchronous and asynchronous exception
    /// handler lists. Before this is called, no <see cref="Err"/> is notified to the handlers.
    /// After this is called, no new handlers can be added, and <see cref="Err"/> is notified to
    /// the handlers.
    /// NOTE: This feature is enabled via the command line argument:
    /// <c>--github.sttk.errs.notify=true</c>
    /// </summary>
    public static void FixHandlers()
    {
        if (!_useNotification) return;
        if (_isHandlersFixed) return;
        _isHandlersFixed = true;
        _syncErrHandlers.TrimExcess();
        _asyncErrHandlers.TrimExcess();
    }

    private static void notifyErr(Err err)
    {
        if (!_useNotification) return;
        if (!_isHandlersFixed) return;

        if (_syncErrHandlers.Count == 0 && _asyncErrHandlers.Count == 0)
            return;

        var tm = DateTimeOffset.Now;

        foreach (var handler in _syncErrHandlers)
        {
            handler(err, tm);
        }

        foreach (var handler in _asyncErrHandlers)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await handler(err, tm);
                }
#if DEBUG
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Async handler failed: {ex}");
                }
#else
                catch
                {
                }
#endif
            });
        }
    }
}
