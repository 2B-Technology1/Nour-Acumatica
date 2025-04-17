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
	/// A earning type which increases the applicable wage for a certain deduction or benefit.
	/// </summary>
	[PXCacheName(Messages.PRDeductCodeEarningIncreasingWage)]
	[Serializable]
	public class PRDeductCodeEarningIncreasingWage : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRDeductCodeEarningIncreasingWage>.By<deductCodeID, applicableTypeCD>
		{
			public static PRDeductCodeEarningIncreasingWage Find(PXGraph graph, int? deductCodeID, string applicableTypeCD, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, deductCodeID, applicableTypeCD, options);
		}

		public static class FK
		{
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRDeductCodeEarningIncreasingWage>.By<deductCodeID> { }
			public class EarningType : EPEarningType.PK.ForeignKeyOf<PRDeductCodeEarningIncreasingWage>.By<applicableTypeCD> { }
		}
		#endregion

		#region DeductCodeID
		public abstract class deductCodeID : BqlInt.Field<deductCodeID> { }
		/// <summary>
		/// The unique identifier of the deduction or benefit code.
		/// The field is included in <see cref="FK.DeductionCode"/>.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PRDeductCode.codeID))]
		[PXParent(typeof(Select<PRDeductCode, Where<PRDeductCode.codeID, Equal<Current<deductCodeID>>>>))]
		public virtual int? DeductCodeID { get; set; }
		#endregion
		#region ApplicableTypeCD
		public abstract class applicableTypeCD : BqlString.Field<applicableTypeCD> { }
		/// <summary>
		/// The unique identifier of the earning type.
		/// The field is included in <see cref="FK.EarningType"/>.
		/// </summary>
		[PXDBString(EPEarningType.typeCD.Length, IsKey = true, IsUnicode = true, InputMask = EPEarningType.typeCD.InputMask)]
		[PXDefault]
		[PXUIField(DisplayName = "Earning Type Code")]
		[PREarningTypeSelector(typeof(Where<PRDeductCode.isPayableBenefit.FromCurrent.IsEqual<False>>))]
		[PXSelector(typeof(
			SelectFrom<EPEarningType>
				.CrossJoin<PRSetup>
				.Where<Brackets<PRSetup.enablePieceworkEarningType.IsEqual<True>
						.Or<PREarningType.isPiecework.IsNotEqual<True>>>
					.And<PRDeductCode.isPayableBenefit.FromCurrent.IsEqual<False>>>
				.SearchFor<EPEarningType.typeCD>),
			DescriptionField = typeof(EPEarningType.description))]
		[PXForeignReference(typeof(Field<applicableTypeCD>.IsRelatedTo<EPEarningType.typeCD>))]
		public virtual string ApplicableTypeCD { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public abstract class tStamp : BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
