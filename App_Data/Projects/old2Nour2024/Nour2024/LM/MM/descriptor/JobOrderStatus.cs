using PX.Data;
using PX.Data.BQL;

namespace MyMaintaince
{
    public class JobOrderStatus
    {
        
        public const string Open = "Open";
        public const string Stoped = "Stoped";
        public const string ReOpen = "ReOpen";
        public const string Started = "Started";
        public const string Finished = "Finished";
        public const string Completed = "Completed";
        public const float AllIssued = 0;
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { Open, Completed },
                    new string[] { Open, Completed }) { ; }
        }
        public class open :BqlString.Constant<open>
        {
            public open() : base(Open) { ;}
        }
        public class reOpen :BqlString.Constant<reOpen>
        {
            public reOpen() : base(ReOpen) { ;}
        }
        public class started :BqlString.Constant<started>
        {
            public started() : base(Started) { ;}
        }
         public class stoped :BqlString.Constant<stoped>
        {
            public stoped() : base(Stoped) { ;}
        }
        public class finished :BqlString.Constant<finished>
        {
            public finished() : base(Finished) { ;}
        }
        public class completed :BqlString.Constant<completed>
        {
            public completed() : base(Completed) { ;}
        }
        public class allIssued :BqlFloat.Constant<allIssued>
        {
            public allIssued() : base(AllIssued) { ;}
        }
    }

    public class OpenTimeStatus
    {

        public const string Opened = "Opened";
        public class opened :BqlString.Constant<opened>
        {
            public opened() : base(Opened) { ;}
        }

        public const string Closed = "Close";
        public class closed :BqlString.Constant<closed>
        {
            public closed() : base(Closed) { ;}
        }

    }
}
