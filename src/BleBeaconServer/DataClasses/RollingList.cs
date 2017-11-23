using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BleBeaconServer.DataClasses
{
    public class RollingList<T>: IEnumerable<T>
    {
        private LinkedList<T> _list = new LinkedList<T>();

        public int MaxCount { get; private set; }

        public RollingList(int maxCount)
        {
            if (maxCount <= 0)
                throw new ArgumentException(null, "maxCount");

            MaxCount = maxCount;
        }

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public void Add(T value)
        {
            if(_list.Count == MaxCount)
            {
                _list.RemoveFirst();
            }
            _list.AddLast(value);
        }

        public T Last { get { return _list.Last.Value; } }
        
        public List<T> ToList()
        {
            List<T> list = new List<T>();
            foreach(T item in _list)
            {
                list.Add(item);
            }
            return list;
        }

        /*
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();

                return _list.
            }
        }*/

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /*
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }*/

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
