/*
 * AsyncErrHandler.cs
 * Copyright (C) 2026 Takayuki Sato. All Rights Reserved.
 */

namespace Errs;

using System.Threading.Tasks;

/// <summary>
/// Is a handler that asynchronously handles an <see cref="Err"/> object when it is created.
/// </summary>
/// <param name="err">The <see cref="Err"/> object to handle.</param>
/// <param name="tm">The creation time when the <see cref="Err"/> object occurred.</param>
/// <returns>A task that represents the asynchronous handling operation.</returns>
public delegate Task AsyncErrHandler(
    Err err,
    DateTimeOffset tm);
