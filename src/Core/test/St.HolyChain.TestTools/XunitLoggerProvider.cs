﻿using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace St.HolyChain.TestTools;

public class XunitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;
    private readonly LogLevel _minLevel;
    private readonly DateTimeOffset? _logStart;

    public XunitLoggerProvider(ITestOutputHelper output)
        : this(output, LogLevel.Trace)
    {
    }

    public XunitLoggerProvider(ITestOutputHelper output, LogLevel minLevel)
        : this(output, minLevel, null)
    {
    }

    public XunitLoggerProvider(ITestOutputHelper output, LogLevel minLevel, DateTimeOffset? logStart)
    {
        _output = output;
        _minLevel = minLevel;
        _logStart = logStart;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(_output, categoryName, _minLevel, _logStart);
    }

    public void Dispose()
    {
    }
}


