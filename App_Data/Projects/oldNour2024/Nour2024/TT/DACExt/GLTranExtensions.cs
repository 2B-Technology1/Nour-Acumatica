using PX.Common;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects;
using System.Collections.Generic;
using System;

namespace PX.Objects.GL
{
	public class GLTranExt : PXCacheExtension<PX.Objects.GL.GLTran>
	{
			#region UsrSerialNbr
			[PXDBInt]
			[PXUIField(DisplayName="Serial Nbr",Enabled=false)]
			[PXDefault(0)]
			public virtual int? UsrSerialNbr { get; set; }
			public abstract class usrSerialNbr : IBqlField { }
			#endregion
	}
}