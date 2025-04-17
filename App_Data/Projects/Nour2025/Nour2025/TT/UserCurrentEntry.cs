using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance
{
    public class UserCurrentEntry : PXGraph<UserCurrentEntry, UserCurrent>
    {
        public PXSelect<UserCurrent> Currents;
    }
}