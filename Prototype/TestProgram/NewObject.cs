using System;

namespace TestProgram
{
    internal class NewObject
    {
        private int _c;

        public int C
        {
            get
            {
                Console.WriteLine("GetC");
                return _c;
            }
            set
            {
                Console.WriteLine("SetC");
                _c = value;
            }
        }

        public NewObject(int i)
        {
            _c = i;
        }
    }
}
