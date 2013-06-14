namespace SoundFingerprinting.Hashing.NeuralHashing.MMI
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///   A group of indeces that represents the hash function
    /// </summary>
    [Serializable]
    public class MinimalMutualInfoGroup
    {
        private readonly List<int> _members = new List<int>();
        private int _groupSize;

        #region Propreties

        /// <summary>
        ///   Gets the element of the hash function that coresponds 
        ///   to a network output on the specified index
        /// </summary>
        /// <param name = "index">Index to get the element at</param>
        /// <returns>The element</returns>
        public int this[int index]
        {
            get
            {
                if (index >= _members.Count)
                    throw new ArgumentException("Index is outside of bounds");
                return _members[index];
            }
        }

        /// <summary>
        ///   Group size
        /// </summary>
        public int GroupSize
        {
            get { return _groupSize; }
            set { _groupSize = value; }
        }

        /// <summary>
        ///   Number of elements currently in the group
        /// </summary>
        public int Count
        {
            get { return _members.Count; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///   Parameterless constructor
        ///   Default group size is 22
        /// </summary>
        public MinimalMutualInfoGroup()
        {
            _groupSize = 22;
            _members = new List<int>();
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "groupSize">Size of the group [22]</param>
        public MinimalMutualInfoGroup(int groupSize)
        {
            _groupSize = groupSize;
        }

        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name = "groupSize">Size of the group [22]</param>
        /// <param name = "startMemberIndex">First element, Highest unconditional entropy</param>
        public MinimalMutualInfoGroup(int groupSize, int startMemberIndex)
        {
            _groupSize = groupSize;
            _members.Add(startMemberIndex);
        }

        #endregion

        /// <summary>
        ///   Tries to add an element to the group
        /// </summary>
        /// <param name = "memberIndex">Element to add</param>
        /// <returns>If the addition was succesfull, return true, otherwise - false</returns>
        public bool AddToGroup(int memberIndex)
        {
            if (_members.Count < _groupSize)
            {
                _members.Add(memberIndex);
                return true;
            }
            return false;
        }

        public bool Contains(int element)
        {
            return _members.Contains(element);
        }

        public void Clear()
        {
            _members.Clear();
        }
    }
}