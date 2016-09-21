using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Joo.Database.Events
{
    public class StatusChangedEventArgs:EventArgs
    {
        public Status OldStatus
        {
            get;
            set;
        }

        public Status NewStatus
        {
            get;
            set;
        }
    }
}
