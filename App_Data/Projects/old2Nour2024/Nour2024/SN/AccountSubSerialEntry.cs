using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace MyMaintaince.SN
{
    public class AccountSubSerialEntry : PXGraph<AccountSubSerialEntry>
    {
        public PXSave<AccountSubSerials> Save;
        public PXCancel<AccountSubSerials> Cancel;
        public PXSelect<AccountSubSerials> accountSubSerials;

    }
}