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

using PX.Data;
using PX.Data.BQL;
using System;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.Localizations.GB.HMRC.DAC
{
	/// <summary>
	/// Business account that is used to file VAT with the MTD (Making Tax Digital) application.
	/// </summary>
	[Serializable()]
	[PXCacheName("MTD External Application")]
	public partial class BAccountMTDApplication : IBqlTable
	{
		/// <summary>
		/// Primary key
		/// </summary>
		public class PK : PrimaryKeyOf<BAccountMTDApplication>.By<bAccountID>
		{
			public static BAccountMTDApplication Find(PXGraph graph, int? bAccountID) => FindBy(graph, bAccountID);
		}

		#region ApplicationID
		/// <summary>
		/// Application ID
		/// </summary>
		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "MTD External Application")]
		public int? ApplicationID { get; set; }
		public abstract class applicationID : PX.Data.BQL.BqlInt.Field<applicationID> { }
		#endregion

		#region BAccountID
		/// <summary>
		/// Business account ID
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Null)]		
		public int? BAccountID { get; set; }
		public abstract class bAccountID : BqlInt.Field<bAccountID> { }
		#endregion
	}
}
