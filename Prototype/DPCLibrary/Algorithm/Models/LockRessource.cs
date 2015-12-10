using System;

namespace DPCLibrary.Algorithm.Models
{
    class LockRessource : IComparable<LockRessource>
    {
        public int Id { get; }

        public LockRessource(int id)
        {
            Id = id;
        }

        public int CompareTo(LockRessource other)
        {
            return Id.CompareTo(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LockRessource))
            {
                return false;
            }
            return Id.Equals(((LockRessource)obj).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static implicit operator LockRessource(int id)
        {
            return new LockRessource(id);
        }
    }
}
