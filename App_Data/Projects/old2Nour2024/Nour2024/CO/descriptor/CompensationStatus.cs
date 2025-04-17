using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;
using PX.SM;
using PX.Data.BQL;

namespace Maintenance.CO
{

    public class CompensationStatus 
    {

            
            public const string NewC = "NEW";
            public const string Printed = "PRINTED";
            public const string Received = "RECEIVED";
            public const string Sended = "SENDED";
            public const string RecievedMaintInvoice = "RecievedMaintInvoice";
            public const string RecievedMaintInvoiceToCompany = "RecievedMaintInvoiceToCompany";
            public const string CheckRecieved = "CheckRecieved";
            public const string CheckDelivered = "CheckDelivered";
            public const string Closed = "Closed";
            public const string Released = "RELEASED";
            

            
            public class newC : BqlString.Constant<newC>
            {
                public newC() : base(NewC) { ;}
            }
            public class printed:BqlString.Constant<printed>
            {
                public printed() : base(Printed) { ;}
            }
            public class sended :BqlString.Constant<sended>
            {
                public sended() : base(Sended) { ;}
            }
            public class received :BqlString.Constant<received>
            {
                public received() : base(Received) { ;}
            }
            public class recievedMaintInvoice :BqlString.Constant<recievedMaintInvoice>
            {
                public recievedMaintInvoice() : base(RecievedMaintInvoice) { ;}
            }
            public class recievedMaintInvoiceToCompany :BqlString.Constant<recievedMaintInvoiceToCompany>
            {
                public recievedMaintInvoiceToCompany() : base(RecievedMaintInvoiceToCompany) { ;}
            }
            public class checkRecieved :BqlString.Constant<checkRecieved>
            {
                public checkRecieved() : base(CheckRecieved) { ;}
            }
            public class checkDelivered :BqlString.Constant<checkDelivered>
            {
                public checkDelivered() : base(CheckDelivered) { ;}
            }
            public class closed :BqlString.Constant<closed>
            {
                public closed() : base(Closed) { ;}
            }
            public class released :BqlString.Constant<released>
            {
                public released() : base(Released) { ;}
            }
            
            
        }

}
