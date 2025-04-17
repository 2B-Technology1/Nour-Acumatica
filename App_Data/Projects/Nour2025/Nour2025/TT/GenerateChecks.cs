using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CA;


namespace Maintenance
{
    public class GenerateChecks : PXGraph<GenerateChecks>
    {
        [Serializable]
        public class ProcessFilter : IBqlTable
        {
            public abstract class cashAccountID : PX.Data.IBqlField
            {
            }
            [PXInt(IsKey=true)]
            [PXUIField(DisplayName = "Cash Account")]
            [PXSelector(typeof(Search<CashAccount.cashAccountID,Where<CashAccountExt.usrCheckDispersant,Equal<True>>>),
                        new Type[]
            {
                  typeof(CashAccount.cashAccountCD),
              typeof(CashAccount.descr),
              typeof(CashAccount.curyID),
            },
                        SubstituteKey = typeof(CashAccount.cashAccountCD))]
            public virtual int? CashAccountID { get; set; }


            #region FromNo
            public abstract class fromNo : PX.Data.IBqlField
            {
            }
            protected string _FromNo;
            [PXDBString(50, IsUnicode = true)]
            [PXUIField(DisplayName = "From")]
            public virtual string FromNo
            {
                get;
                set;
            }
            #endregion

            #region To
            public abstract class toNo : PX.Data.IBqlField
            {
            }
            protected string _ToNo;
            [PXDBString(50, IsUnicode = true)]
            [PXUIField(DisplayName = "To")]
            public virtual string ToNo
            {
                get;
                set;
            }
            #endregion
        }

        public PXCancel<ProcessFilter> Cancel;

        public PXAction<ProcessFilter> Generate;
        [PXProcessButton]
        [PXUIField(DisplayName = "Generate")]
        protected virtual IEnumerable generate(PXAdapter adapter)
        {
            List<CheckNo> list = new List<CheckNo>();
            Int64 from = Int64.Parse(this.Data.Current.FromNo);
            Int64 to = Int64.Parse(this.Data.Current.ToNo);

            //Actions.PressSave();
            PXLongOperation.StartOperation(this, delegate()
            {
                for (Int64 x = from; x <= to; x++)
                {
                    CheckNoEntry check = PXGraph.CreateInstance<CheckNoEntry>();
                    check.check.Insert();
                    check.check.Current.CashAccountID = this.Data.Current.CashAccountID;
                    check.check.Current.CheckNumber = x.ToString();
                    check.check.Current.Used = false;
                    CheckNo exist = PXSelect<CheckNo, Where<CheckNo.cashAccountID, Equal<Required<CheckNo.cashAccountID>>, And<CheckNo.checkNumber, Equal<Required<CheckNo.checkNumber>>>>>.Select(this,check.check.Current.CashAccountID, check.check.Current.CheckNumber);
                    if (exist == null)
                    {
                        check.Actions.PressSave();
                        list.Add(check.check.Current);
                    }

                    
                }
            });
            return list;
        }

        public PXSelect<CheckNo> Filter;

        public PXFilter<ProcessFilter> Data;

        
    }
}