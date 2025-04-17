using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Maintenance
{
    public class CashAccountAccessEntry : PXGraph<CashAccountAccessEntry>
    {

        public PXCancel<CashAccountAccess> Cancel;
        public PXSave<CashAccountAccess> Save;

        //public class UserList : IBqlTable
        //{
        //    public abstract class userName : PX.Data.IBqlField
        //    {
        //    }
        //    [PXInt]
        //    [PXUIField(DisplayName = "User")]
        //    [PXSelector
        //        (typeof(Search<Users.username>),
        //                new Type[]
        //                {
        //                    typeof(Users.username),typeof(Users.fullName)
        //                },
        //        DescriptionField = typeof(Users.fullName)//,
        //        //SubstituteKey = typeof(CashAccount.cashAccountCD)
        //        )]
        //    public virtual int? UserName { get; set; }
        //}
        //[PXFilterable]

        //public PXSelect<CashAccountAccess> UserNames;

        public PXSelect<CashAccountAccess> Accounts;
        /*,Where<CashAccountAccess.userID,Equal<Current<Users.pKID>>>*/
    }
}