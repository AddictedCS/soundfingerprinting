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
        private readonly List<int> members = new List<int>();
        private int groupSize;

        #region Constructors

        public MinimalMutualInfoGroup()
        {
            groupSize = 22;
            members = new List<int>();
        }

        public MinimalMutualInfoGroup(int groupSize)
        {
            this.groupSize = groupSize;
        }

        public MinimalMutualInfoGroup(int groupSize, int startMemberIndex)
        {
            this.groupSize = groupSize;
            members.Add(startMemberIndex);
        }

        #endregion

        #region Propreties

        public int GroupSize
        {
            get { return groupSize; }
            set { groupSize = value; }
        }

        public int Count
        {
            get { return members.Count; }
        }

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
                if (index >= members.Count)
                {
                    throw new ArgumentException("Index is outside of bounds");
                }

                return members[index];
            }
        }

        #endregion

        /// <summary>
        ///   Tries to add an element to the group
        /// </summary>
        /// <param name = "memberIndex">Element to add</param>
        /// <returns>If the addition was succesfull, return true, otherwise - false</returns>
        public bool AddToGroup(int memberIndex)
        {
            if (members.Count < groupSize)
            {
                members.Add(memberIndex);
                return true;
            }

            return false;
        }

        public bool Contains(int element)
        {
            return members.Contains(element);
        }

        public void Clear()
        {
            members.Clear();
        }
    }
}