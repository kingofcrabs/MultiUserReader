using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadResult
{
    public class Invoker
    {
        public Action<string> sDel;

        private MainWindow owner;

        public Invoker(MainWindow wOwner)
        {
            this.owner = wOwner;
        }

        public void Invoke(string sArg)
        {
            this.owner.Dispatcher.Invoke(this.sDel, new object[]
			{
				sArg
			});
        }

        public void BeginInvoke(string sArg)
        {
            this.owner.Dispatcher.BeginInvoke(this.sDel, new object[]
			{
				sArg
			});
        }
    }
}
