using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.EP;
using PX.Objects.FA;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace PX.Objects.AP
{
	public class APTranExt : PXCacheExtension<PX.Objects.AP.APTran>
	{
			#region UsrChassis
			[PXDBString(50)]
			[PXUIField(DisplayName="Chassis",Enabled=false)]
			public virtual string UsrChassis { get; set; }
			public abstract class usrChassis : IBqlField { }
			#endregion
	}
}