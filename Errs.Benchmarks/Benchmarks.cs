using System;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace Errs.Benchmarks;

public class Benchmarks
{
    record FailToDoSomething(string name, string flag, string type);

    [Benchmark]
    public void TestNewErr()
    {
        try
        {
            throw new Err(new FailToDoSomething("foo", "bar", "baz"));
        }
        catch
        {
        }
    }

    [Benchmark]
    public void TestNewErrWithInnrException()
    {
        try
        {
            var cause = new IndexOutOfRangeException("4 is out of range: 0-3");
            throw new Err(new FailToDoSomething("foo", "bar", "baz"), cause);
        }
        catch
        {
        }
    }

    [Benchmark]
    public void TestIdentifyReasonWithSwitch()
    {
        try
        {
            throw new Err(new FailToDoSomething("foo", "bar", "baz"));
        }
        catch (Err err)
        {
            switch (err.Reason)
            {
                case FailToDoSomething:
                    break;
                default:
                    break;
            }
        }
    }

    [Benchmark]
    public void TestToString()
    {
        try
        {
            throw new Err(new FailToDoSomething("foo", "bar", "baz"));
        }
        catch (Err err)
        {
            _ = err.ToString();
        }
    }
}
