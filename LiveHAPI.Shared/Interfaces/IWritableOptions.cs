﻿using System;
using Microsoft.Extensions.Options;

namespace LiveHAPI.Shared.Interfaces
{
    public interface IWritableOptions<out T> : IOptionsSnapshot<T> where T : class, new()
    {
        void Update(Action<T> applyChanges);
    }
}