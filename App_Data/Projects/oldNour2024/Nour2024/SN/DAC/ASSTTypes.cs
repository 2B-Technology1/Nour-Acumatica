using PX.Data;
using PX.Data.BQL;

namespace MyMaintaince.SN
{
    public class ASSTTypes
    {
        public const string Debit = "D";
        public const string Credit = "C";

        public class debit : BqlString.Constant<debit>
        {
            public debit() : base(Debit) { ;}
        }

        public class credit : BqlString.Constant<credit>
        {
            public credit() : base(Credit) { ;}
        }
    }
}
