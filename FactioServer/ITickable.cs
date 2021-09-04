using System;
using System.Collections.Generic;
using System.Text;

namespace FactioServer
{
    public interface ITickable
    {
        public void Tick(long id);
    }
}
