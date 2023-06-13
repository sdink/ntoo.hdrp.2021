using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ntoo.ExtendedBuffer
{
  /// <inheritdoc/>
  /// <summary>
  /// Extended buffer.
  /// </summary>
  public class ExtendedBuffer<T> : IEnumerable<T>
  {
    private T[] _buffer;

    /// <summary>
    /// The _size. Number of meaningful entries into buffer (may not equal total capacity).
    /// </summary>
    private int _size;

    /// <summary>
    /// Maximum capacity of the buffer. Elements pushed into the buffer after
    /// maximum capacity is reached, will be rejected.
    /// </summary>
    private int _capacity;

    /// <summary>
    /// Whether the _capacity is _locked or whether the buffer may grow.
    /// </summary>
    private bool _locked;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedBuffer{T}"/> class.
    /// 
    public ExtendedBuffer()
        : this(0, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedBuffer{T}"/> class.
    /// 
    /// </summary>
    /// <param name='capacity'>
    /// Buffer capacity. Must be positive.
    /// </param>
    public ExtendedBuffer(int capacity)
        : this(capacity, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedBuffer{T}"/> class.
    /// 
    /// </summary>
    /// <param name='capacity'>
    /// Buffer capacity. Must be positive.
    /// </param>
    /// <param name='items'>
    /// Items to fill buffer with. Items length must be less than capacity.
    /// Suggestion: use Skip(x).Take(y).ToArray() to build this argument from
    /// any enumerable.
    /// </param>
    public ExtendedBuffer(int capacity, bool locked)
    {
      _capacity = capacity;
      _size = 0;
      _buffer = new T[_capacity];
      _locked = locked;
    }

    /// <summary>
    /// True if has no elements.
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return Size == 0;
      }
    }

    /// <summary>
    /// Total capacity of buffer (the current size of the buffer, and if locked, the maximum size).
    /// </summary>
    public int Capacity { get { return _capacity; } }

    /// <summary>
    /// Current buffer size (the number of elements that the buffer has).
    /// </summary>
    public int Size { get { return _size; } }

    /// <summary>
    /// Whether the buffer is locked from growing.
    /// </summary>
    public bool Locked { get { return _locked; } }

    /// <summary>
    /// Index access to elements in buffer.
    /// Index does not loop around like when adding elements,
    /// valid interval is [0;Size[
    /// </summary>
    /// <param name="index">Index of element to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when index is outside of [; Size[ interval.</exception>
    public T this[int index]
    {
      get
      {
        if (IsEmpty)
        {
          throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer is empty");
        }
        if (index >= _size)
        {
          throw new IndexOutOfRangeException($"Cannot access index {index}. Buffer size is {_size}");
        }
        return _buffer[index];
      }
      set
      {
        if (index >= _capacity)
        {
          if (_locked) throw new IndexOutOfRangeException($"Cannot write to index {index} as buffer is locked. Buffer size is {_size}");

          T[] oldBuffer = _buffer;
          _capacity = index + 1;
          _buffer = new T[_capacity];
          // Add values up to old buffer's capacity (i.e. -1).
          for (int i = 0; i < _capacity - 1; i++)
          {
            _buffer[i] = oldBuffer[i];
          }
          // Add null values up til index.
          for (int i = _capacity - 1; i < index; i++)
          {
            _buffer[i] = default(T);
          }
          // Set size to capacity and new value to index.
          _size = _capacity;
          _buffer[index] = value;
        }
        else
        {
          // If index is within capacity but above size, update it.
          if (index >= _size)
          {
            _size = index + 1;
          }
          _buffer[index] = value;
        }
      }
    }

    /// <summary>
    /// Pushes a new element to the back of the buffer. Back()/this[Size-1]
    /// will now return this element.
    /// 
    /// </summary>
    /// <param name="item">Item to push to the back of the buffer</param>
    public void Push(T item)
    {
      this[_size] = item;
    }

    /// <summary>
    /// Pops the element from the front of the buffer. Decreasing the 
    /// Buffer size by 1.
    /// </summary>
    public void Pop()
    {
      ThrowIfEmpty("Cannot take elements from an empty buffer.");

      // Create new buffer with n-1 size
      T[] oldBuffer = _buffer;
      _size--;
      _buffer = new T[_size];

      // Add old values, skipping first item.
      for (int i = 1; i < _size + 1; i++)
      {
        _buffer[i - 1] = oldBuffer[i];
      }
    }

    /// <summary>
    /// Clears the contents of the array. Size = 0, Capacity is unchanged.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Clear()
    {
      // To clear we just reset everything.
      _size = 0;
      Array.Clear(_buffer, 0, _buffer.Length);
    }

    /// <summary>
    /// Copies the buffer contents to an array, according to the logical
    /// contents of the buffer (i.e. independent of the internal 
    /// order/contents)
    /// </summary>
    /// <returns>A new array with a copy of the buffer contents.</returns>
    public T[] ToArray()
    {
      T[] newArray = new T[Size];
      Array.Copy(_buffer, newArray, Size);
      return newArray;
    }
    /// <summary>
    /// Returns an enumerator that iterates through this buffer.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate this collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
      for (int i = 0; i < Size; i++)
      {
        yield return _buffer[i];
      }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)GetEnumerator();
    }

    private void ThrowIfEmpty(string message = "Cannot access an empty buffer.")
    {
      if (IsEmpty)
      {
        throw new InvalidOperationException(message);
      }
    }
  }
}