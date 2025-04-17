using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects;
using PX.TM;
using System.Collections.Generic;
using System;

namespace PX.Objects.IN
{
	public class InventoryItemExt : PXCacheExtension<PX.Objects.IN.InventoryItem>
	{
			#region UsrInvoiceDesc
			[PXDBString(300,IsUnicode = true)]
			[PXUIField(DisplayName="Invoice Desc.")]

			public virtual string UsrInvoiceDesc { get; set; }
			public abstract class usrInvoiceDesc : IBqlField { }
			#endregion
	}
}