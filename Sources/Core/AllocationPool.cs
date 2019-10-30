using System;
using System.Collections.Generic;
using System.Linq;

namespace NoXP.Scrcpy
{

    public class AllocationPool<T>
    {
        private HashSet<T> _deallocated = null; // new HashSet<int>(Enumerable.Range(5555, 64));
        private HashSet<T> _allocated = new HashSet<T>();

        public AllocationPool(IEnumerable<T> collection)
        {
            _deallocated = new HashSet<T>(collection);
        }

        public bool Add(T deallocated)
        { return _deallocated.Add(deallocated); }
        public void AddRange(IEnumerable<T> deallocated)
        { _deallocated.UnionWith(deallocated); }

        public T Allocate()
        {
            if (_deallocated.Count == 0)
                throw new ArgumentException("There are no entries left. Please return one used entry back to the unused set befor retrieving an unused.");

            T result = _deallocated.First();
            _deallocated.Remove(result);
            _allocated.Add(result);
            return result;
        }
        public void Deallocate(T allocated)
        {
            if (_allocated.Contains(allocated))
            {
                _allocated.Remove(allocated);
                _deallocated.Add(allocated);
            }
            else
                throw new ArgumentException("The value to deallocate is not present in the allocated set. A value not allocated can not be deallocated.");
        }

    }

}