# [Errs][repo-url] [![NuGet Repository][nuget-img]][nuget-url] [![GitHub.io][io-img]][io-url] [![CI Status][ci-img]][ci-url] [![MIT license][mit-img]][mit-url]

A library for handling errors with reasons for C#

## Overview

`errs` is an exception handling library for C# designed to focus on the "Reason" behind an error.

### Expressing "Why It Failed" via the Type System

Instead of scattering many small exception subclasses across your codebase, `errs` uses a single `Err` exception class that carries a “Reason” object whose type represents why the failure occurred.

For error reasons, you can use anything from lightweight types like `string` to type-safe definitions using `record`s, all handled flexibly with the same API.
By using a `record` in particular, you can not only express failure factors within the type system but also hold contextual information in its fields, propagating the context and relevant data at the time of the error as-is.
Furthermore, since reasons can be determined in a type-safe manner using pattern-matching `switch` expressions, you can avoid fragile error handling that relies on string comparisons.

### Decentralized Error Definition and Traceability

`errs` encourages defining error reasons close to where they occur.
This eliminates the need to share a massive, monolithic error message management across the entire application, enabling a highly maintainable design while keeping dependencies between classes clean.
Type information is utilized to identify the reason, and the type identifiers required for this determination are resolved statically at compile time. This provides type-safe error handling with minimal runtime overhead.

The core `Err` type of the library inherits `System.Exception`, allowing it to integrate naturally with standard C# exception handling.
It can also retain lower-layer exceptions as causes, enabling you to manage the "Reason" of the upper layer and the "InnerException" of the lower layer separately.
Additionally, it automatically records the file name and line number when an error is generated, making log output and failure analysis effortless.

### Powerful Error-Instantiation Notification & Monitoring Ecosystem

Furthermore, `errs` features a mechanism to notify error generation events.
By running with the command line argument `--github.sttk.errs.notify=true`, an automatic notification can be sent to registered handlers the exact moment an `Err` is created.
It supports synchronous handlers and asynchronous handlers, and it accommodates registration within functions.
This makes it easy to implement logging, monitoring, metrics collection, and integration with telemetry systems.

While standard C# exceptions often focus primarily on annotating and propagating errors, `errs` emphasizes explicitly defining the reason for failure through types and reliably observing the exact moment it occurs.
This library is ideal for scenarios where you want to tightly manage the semantics of errors within your application while seamlessly integrating with production monitoring and operational infrastructure.


## Install

This package can be installed from [NuGet][nuget-url].

In your project file, write this package as a dependency.

```xml
<PackageReference Include="Errs" Version="0.1.0" />
```

You can also install this package with `dotnet` command, as follows.

```bash
dotnet add package Errs --version 0.1.0
```


## Usage

### Locally Defined Reasons and Instantiate an Err with Them

An `Err` class can be instantiated with any arbitrary error reason. Typically, a record defined to indicate the cause or context of the error is used as the reason. This reason does not need to be declared in a centralized file of global errors; rather, it is preferable to define it close to where the error using it as a reason actually occurs.

```c#
using Errs;

public class SampleClass
{
    record IndexOutOfRange(String name, int index, int min, int max);

    public void SampleMethod()
    {
        // ...
        throw new Err(new IndexOutOfRange("array", i, 0, array.Length));
    }
}
```

An `Err` can also be instantiated with the underlying inner exception along with the reason.

```c#
    public void SampleMethod()
    {
        try
        {
            // ...
        }
        catch (IOException e)
        {
            throw new Err(new IndexOutOfRange("array", i, 0, array.Length), e);
        }
    }
```

### Type-Safe Reason Identification

By using the pattern-matching switch expression, you can extract the error reason as the specified type.

```c#
  try
  {
      SampleMethod();
  }
  catch (Err e)
  {
      switch (e.Reason)
      {
          case IndexOutOfRange reason:
              string nam = reason.Name;
              int index = reason.Index;
              int min = reason.Min;
              int max = reason.Max;
              ...
              break;

          default:
              ...
              break;
      }
  }
```

### Error Handler Registration

> To enable this feature, you must specify the command line argument `--github.sttk.errs.notify=true` at program startup.

This library optionally provides a feature to notify pre-registered error handlers when an `Err` is instantiated.
Multiple error handlers can be registered, and you can choose to receive notifications either synchronously or asynchronously.

To register handlers, you can use the following functions:

```c#
using Errs;

public class Program
{
    static Program()
    {
        Err.AddSyncHandler((err, tm) =>
        {
          Console.WriteLine(string.Format("{0} - {1}:{2}",
            err.Message, err.File, err.Line);
        });

        Err.AddAsyncHandler((err, tm) =>
        {
          remoteLogger.Log(string.format("{0}:{1}:{2}:{3}",
              tm.ToString("yyyy-MM-dd'T'HH:mm:sszzz"), err.File, err.Line, err.ToString()));
        });

        Err.FixHandlers();
    }

    record IndexOutOfRange(String name, int index, int min, int max);

    public static void Main()
    {
        try
        {
            throw new Err(new IndexOutOfRange("array", 11, 0, 10));
        }
        catch (Err e)
        {
            ...
        }
    }
}
```

```bash
% MyApp --github.sttk.errs.notify=true
Program.IndexOutOfRange { name = array, index = 11, min = 0, max = 10 } - Program.cs:27
```

Error notifications will not occur until the `Err.FixHandlers` static method is called.
This static method locks the current set of error handlers, preventing further additions and enabling notification processing.


## Native build

This library supports native build.

### Actually test results

```bash
% ./build.sh native-test
Restore complete (1.4s)
    Determining projects to restore...
    All projects are up-to-date for restore.
  Errs net10.0 succeeded (0.4s) → Errs/bin/Release/net10.0/Errs.dll
  Errs.NativeTests net10.0 osx-x64 succeeded (25.7s) → Errs.NativeTests/bin/Release/net10.0/osx-x64/publish/
    Generating native code

Build succeeded in 28.0s
xUnit.net v3 In-Process Runner v4.0.1+8ed8aa354c [native/osx-x64] (.NET 10.0.12)
  Discovering: Errs.NativeTests
  Discovered:  Errs.NativeTests
  Starting:    Errs.NativeTests
  Finished:    Errs.NativeTests (ID = 'd258c38db83912629e56c76208240907560d31a28042a0415e1e9e44b66c462b')
=== TEST EXECUTION SUMMARY ===
   Errs.NativeTests  Total: 18, Errors: 0, Failed: 0, Skipped: 0, Not Run: 0, Time: 0.002s
```

## License

Copyright (C) 2026 Takayuki Sato

This program is free software under MIT License.<br>
See the file LICENSE in this distribution for more details.


[repo-url]: https://github.com/sttk/errs-csharp
[nuget-img]: https://img.shields.io/badge/NuGet-0.1.0-6600ff.svg
[nuget-url]: https://nuget.org/packages/Errs
[ci-img]: https://github.com/sttk/errs-csharp/actions/workflows/csharp.yml/badge.svg?branch=main
[ci-url]: https://github.com/sttk/errs-csharp/actions?query=branch%3Amain
[io-img]: https://img.shields.io/badge/github.io-docfx-4c69fd.svg
[io-url]: https://sttk.github.io/errs-csharp/api/Errs.html
[mit-img]: https://img.shields.io/badge/license-MIT-green.svg
[mit-url]: https://opensource.org/licenses/MIT
