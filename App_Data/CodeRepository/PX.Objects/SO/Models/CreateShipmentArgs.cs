/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.SO
{
	/// <exclude/>
	public class CreateShipmentArgs
	{
		public SOOrderEntry Graph { get; set; }
		public bool MassProcess { get; set; }
		public SOOrder Order { get; set; }
		public int? OrderLineNbr { get; set; }
		public int? SiteID { get; set; }
		public DateTime? ShipDate { get; set; }
		public bool? UseOptimalShipDate { get; set; }
		public string Operation { get; set; }
		public DocumentList<SOShipment> ShipmentList { get; set; }
		public PXQuickProcess.ActionFlow QuickProcessFlow { get; set; } = PXQuickProcess.ActionFlow.NoFlow;
	}
}
