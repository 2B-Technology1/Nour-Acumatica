using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace MyMaintaince.LM
{
    public class GOVTrafficUnitsEntry : PXGraph<GOVTrafficUnitsEntry,Governrate>
    {
        public PXSelect<Governrate> governrate;
        public PXSelect<TrafficUnits, Where<TrafficUnits.govID, Equal<Current<Governrate.govID>>>> trafficUnits;
    }
}