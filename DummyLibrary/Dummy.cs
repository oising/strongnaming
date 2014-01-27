using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DummyReferenceLibrary;

namespace DummyLibrary
{
    public class Dummy
    {
        private readonly Bar _bar = new Bar();

        public void Foo() { Console.WriteLine(_bar.ToString()); }
    }
}
