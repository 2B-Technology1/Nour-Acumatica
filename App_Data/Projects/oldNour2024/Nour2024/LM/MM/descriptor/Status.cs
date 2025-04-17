using PX.Data;
using PX.Data.BQL;


namespace MyMaintaince.descriptor
{
    public class Status : PX.Data.IBqlTable
    {

        public abstract class OpenOrCompleted : PX.Data.IBqlField
        {

            public const string Open = "Open";
            public const string Started = "Started";
            public const string Finished = "Finished";
            public const string Completed = "Completed";
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                        new string[] { Open, Completed },
                        new string[] { Open, Completed }) { ; }
            }
            public class open : BqlString.Constant<open>
            {
                public open() : base(Open) { ;}
            }
            public class started : BqlString.Constant<started>
            {
                public started() : base(Started) { ;}
            }
            public class finished : BqlString.Constant<finished>
            {
                public finished() : base(Finished) { ;}
            }
            public class completed : BqlString.Constant<completed>
            {
                public completed() : base(Completed) { ;}
            }
        }
    }
}