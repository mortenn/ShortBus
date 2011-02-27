using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShortBus.Contracts
{
    public abstract class IFactory<T>
        where T : class
    {
        public abstract T Connect(string endpoint);
    }
}
