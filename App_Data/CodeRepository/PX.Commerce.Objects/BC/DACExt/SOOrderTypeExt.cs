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
using PX.Objects.SO;
using System;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// DAC extension of SOOrderType to add additional properties.
	/// </summary>
	[Serializable]
	public sealed class SOOrderTypeExt : PXCacheExtension<SOOrderType>
	{
		#region EncryptAndPseudonymizePII
		/// <summary>
		/// Indicates whether to encrypt personal identifying information.
		/// </summary>
		[PXDBBool]
		[PXUIVisible(typeof(Where<Current<SOOrderType.behavior>, Equal<SOBehavior.iN>, Or<Current<SOOrderType.behavior>,Equal<SOBehavior.sO>>>))]
		[PXUIField(DisplayName = "Protect Personal Data", Visible = false)]
		public  bool? EncryptAndPseudonymizePII { get; set; }
		/// <inheritdoc cref="EncryptAndPseudonymizePII"/>
		public abstract class encryptAndPseudonymizePII : PX.Data.BQL.BqlBool.Field<encryptAndPseudonymizePII> { }
		#endregion

	}
}
