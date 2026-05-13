using System.Collections;

namespace Backend.Fx.Mediator.Feature.Diagnostics;

public class ThreadSafeBoundedQueue<T> : IEnumerable<T>
{
    private readonly Queue<T> _queue = new();
    private readonly int _capacity;
    private readonly object _lock = new();

    public ThreadSafeBoundedQueue(int capacity) => _capacity = capacity;

    public void Enqueue(T item)
    {
        lock (_lock)
        {
            if (_queue.Count >= _capacity)
                _queue.Dequeue();
            _queue.Enqueue(item);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (_lock) return _queue.ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}