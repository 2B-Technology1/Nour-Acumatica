using PX.Data;
using PX.Data.BQL;

namespace MyMaintaince.SN
{
    public class WSTTypes
    {
        public const string Issue = "I";
        public const string Receipt = "R";

        public class issue :BqlString.Constant<issue>
        {
            public issue() : base(Issue) { ;}
        }

        public class receipt :BqlString.Constant<receipt>
        {
            public receipt() : base(Receipt) { ;}
        }
    }
}
