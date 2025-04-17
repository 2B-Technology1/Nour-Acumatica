using PX.Data;
using PX.Data.BQL;

namespace MyMaintaince.LM
{
    public class LicenceStatus 
    {

            public const string Open = "Open";
            public const string Printed = "Printed";
            public const string Sent = "Sent";
            public const string Transfered = "Transfered";
            public const string Received = "Received";
            public const string Released = "Released";
            public const string CustomerReceived = "CustomerReceived";

            public class open :BqlString.Constant<open>
            {
                public open() : base(Open) { ;}
            }
            public class printed :BqlString.Constant<printed>
            {
                public printed() : base(Printed) { ;}
            }
            public class sent :BqlString.Constant<sent>
            {
                public sent() : base(Sent) { ;}
            }
            public class transfered :BqlString.Constant<transfered>
            {
                public transfered() : base(Transfered) { ;}
            }
            public class received :BqlString.Constant<received>
            {
                public received() : base(Received) { ;}
            }
            public class released :BqlString.Constant<released>
            {
                public released() : base(Released) { ;}
            }
            public class customerReceived :BqlString.Constant<customerReceived>
            {
                public customerReceived() : base(CustomerReceived) { ;}
            }
        }
}
