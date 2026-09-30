namespace Errs.Tests;

using Errs;
using System.Threading.Tasks;

public class NotificationTest
{
    void Reset()
    {
        Err._useNotification = true;
        Err._isHandlersFixed = false;
        Err._syncErrHandlers.Clear();
        Err._asyncErrHandlers.Clear();
    }

    [Fact]
    void should_and_sync_handlers_and_fix()
    {
        Reset();

        Assert.Empty(Err._syncErrHandlers);

        SyncErrHandler handler1 = (err, tm) => { };
        Err.AddSyncHandler(handler1);

        Assert.Equal(Err._syncErrHandlers, new List<SyncErrHandler>
        {
            handler1
        });

        SyncErrHandler handler2 = (err, tm) => { };
        Err.AddSyncHandler(handler2);

        Assert.Equal(Err._syncErrHandlers, new List<SyncErrHandler>
        {
            handler1,
            handler2
        });

        Err.FixHandlers();

        SyncErrHandler handler3 = (err, tm) => { };
        Err.AddSyncHandler(handler3);

        Assert.Equal(Err._syncErrHandlers, new List<SyncErrHandler>
        {
            handler1,
            handler2
        });
    }

    [Fact]
    void should_and_async_handlers_and_fix()
    {
        Reset();

        Assert.Empty(Err._asyncErrHandlers);

        AsyncErrHandler handler1 = async (err, tm) => { };
        Err.AddAsyncHandler(handler1);

        Assert.Equal(Err._asyncErrHandlers, new List<AsyncErrHandler>
        {
            handler1
        });

        AsyncErrHandler handler2 = (err, tm) => Task.CompletedTask;
        Err.AddAsyncHandler(handler2);

        Assert.Equal(Err._asyncErrHandlers, new List<AsyncErrHandler>
        {
            handler1,
            handler2
        });

        Err.FixHandlers();

        AsyncErrHandler handler3 = async (err, tm) => { };
        Err.AddAsyncHandler(handler3);

        Assert.Equal(Err._asyncErrHandlers, new List<AsyncErrHandler>
        {
            handler1,
            handler2
        });
    }

    record FailToDoSomething(String name);

    [Fact]
    public void should_notify_exception()
    {
        Reset();

        List<String> syncLogs = new();
        List<String> asyncLogs = new();

        Err.AddSyncHandler((err, tm) =>
        {
            if (err.Reason is FailToDoSomething)
            {
                syncLogs.Add(string.Format(
                    "{0}:{1}({2}):{3}",
                    tm.ToString("yyyy/MM/dd HH:mm:dd"),
                    err.File,
                    err.Line,
                    err.Reason
                ));
            }
        });

        Err.AddAsyncHandler(async (err, tm) =>
        {
            if (err.Reason is FailToDoSomething)
            {
                asyncLogs.Add(string.Format(
                    "{0}:{1}({2}):{3}",
                    tm.ToString("yyyy/MM/dd HH:mm:dd"),
                    err.File,
                    err.Line,
                    err.Reason
                ));
            }
        });
        new Err(new FailToDoSomething("abc"));

        Assert.Empty(syncLogs);
        Assert.Empty(asyncLogs);

        Err.FixHandlers();

        new Err(new FailToDoSomething("abc"));
        Assert.Equal(syncLogs.Count(), 1);
        Assert.EndsWith(":NotificationTest.cs(132):FailToDoSomething { name = abc }", syncLogs[0]);

        Thread.Sleep(100);
        Assert.Equal(asyncLogs.Count(), 1);
        Assert.EndsWith(":NotificationTest.cs(132):FailToDoSomething { name = abc }", asyncLogs[0]);
    }
}
