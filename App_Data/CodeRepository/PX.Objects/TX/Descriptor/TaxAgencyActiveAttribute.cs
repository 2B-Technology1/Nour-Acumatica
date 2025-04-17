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
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;

namespace PX.Objects.TX.Descriptor
{
	/// <summary>
	/// Displays only Active or OneTime tax agency
	/// </summary>
	[PXRestrictor(typeof(Where<Vendor.taxAgency, Equal<True>,
						And<Where<Vendor.vStatus, Equal<VendorStatus.active>,
									Or<Vendor.vStatus, Equal<VendorStatus.oneTime>>>>>), 
						Messages.TaxAgencyStatusIs, 
						typeof(Vendor.vStatus))]
	public class TaxAgencyActiveAttribute : VendorAttribute
	{
		public TaxAgencyActiveAttribute(Type search)
			: base(search)
		{
		}

		public TaxAgencyActiveAttribute()
			: base()
		{
		}

		protected override void Initialize()
		{
			base.Initialize();

			DisplayName = "Tax Agency";
			DescriptionField = typeof(Vendor.acctName);
		}
	}
}
