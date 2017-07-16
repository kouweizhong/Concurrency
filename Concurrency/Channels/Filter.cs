using System;

namespace MartinSu.Concurrency.Channels
{
    /// <summary>
    /// Message filter delegate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="msg"></param>
    /// <returns></returns>
    public delegate bool Filter<T>(T msg);
}
