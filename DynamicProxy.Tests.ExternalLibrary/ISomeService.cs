﻿namespace DynamicProxy.Tests.ExternalLibrary
{
    public interface ISomeService
    {
        string GetString(string input);
        int GetInt(int input);
    }
}