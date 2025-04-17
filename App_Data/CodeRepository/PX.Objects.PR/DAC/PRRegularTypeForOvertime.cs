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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.EP;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the regular earning types that are associated with current overtime type. The information will be displayed in the Regular Time tab on the Earning Type Codes (PR102000) form.
	/// </summary>
	[PXCacheName(Messages.PRRegularTypeForOvertime)]
	public class PRRegularTypeForOvertime : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRRegularTypeForOvertime>.By<overtimeTypeCD, regularTypeCD>
		{
			public static PRRegularTypeForOvertime Find(PXGraph graph, string overtimeTypeCD, string regularTypeCD) => FindBy(graph, overtimeTypeCD, regularTypeCD);
		}

		public static class FK
		{
			public class OvertimeEarningType : EPEarningType.PK.ForeignKeyOf<EPEarningType>.By<overtimeTypeCD> { }
			public class RegularEarningType : EPEarningType.PK.ForeignKeyOf<EPEarningType>.By<regularTypeCD> { }
		}
		#endregion

		#region OvertimeTypeCD
		public abstract class overtimeTypeCD : BqlString.Field<overtimeTypeCD> { }
		/// <summary>
		/// The user-friendly unique identifier of the overtime earning type.
		/// The field is included in <see cref="FK.OvertimeEarningType"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPEarningType.TypeCD"/> field.
		/// </value>
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, IsKey = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXDBDefault(typeof(EPEarningType.typeCD))]
		[PXParent(typeof(FK.OvertimeEarningType))]
		public virtual string OvertimeTypeCD { get; set; }
		#endregion

		#region RegularTypeCD
		public abstract class regularTypeCD : BqlString.Field<regularTypeCD> { }
		/// <summary>
		/// The user-friendly unique identifier of the earning type to be used for calculation of the overtime amount.
		/// The field is included in <see cref="FK.RegularEarningType"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="EPEarningType.TypeCD"/> field.
		/// </value>
		[PXDefault]
		[PXDBString(EPEarningType.typeCD.Length, IsUnicode = true, IsKey = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXForeignReference(typeof(FK.RegularEarningType))]
		[PXUIField(DisplayName = "Code")]
		// Only 'Wage', 'Piecework', and 'Time Off' earning types should be shown in this selector.
		// Thus, 'IsOvertime' and 'IsAmountBased' flags should be false.
		// Even though these flags are not nullable, the 'PREarningType' table might be empty if the 'Payroll Module' feature has been just switched on.
		// Since the 'PREarningType' DAC is a CacheExtension of the 'EPEarningType' DAC, a Left Join operator will be applied in a generated SQL code.
		// This is the reason why we need the following condition in SQL:
		// ... AND (IsPiecework IS NULL   OR   IsPiecework = 0) ...
		[PXSelector(typeof(SearchFor<EPEarningType.typeCD>
			.Where<EPEarningType.isActive.IsEqual<True>
				.And<EPEarningType.isOvertime.IsEqual<False>>
				.And<PREarningType.isAmountBased.IsNull
					.Or<PREarningType.isAmountBased.IsEqual<False>>>>),
			DescriptionField = typeof(EPEarningType.description))]
		public virtual string RegularTypeCD { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		#endregion

		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion System Columns
	}
}
