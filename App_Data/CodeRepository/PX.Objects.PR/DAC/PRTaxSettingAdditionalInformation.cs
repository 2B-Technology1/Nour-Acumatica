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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the additional information provided for each tax settings.
	/// The information will be displayed on the Tax Maintenance (PR208000) and the Employee Payroll Settings (PR203000) forms.
	/// </summary>
	[PXCacheName(Messages.PRTaxSettingAdditionalInformation)]
	[Serializable]
	public class PRTaxSettingAdditionalInformation : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRTaxSettingAdditionalInformation>.By<settingName, state, countryID>
		{
			public static PRTaxSettingAdditionalInformation Find(PXGraph graph, string settingName, string state, string countryID, PKFindOptions options = PKFindOptions.None) => 
				FindBy(graph, settingName, state, countryID, options);
		}
		#endregion

		#region TypeName
		public abstract class typeName : PX.Data.BQL.BqlString.Field<typeName> { }
		[PXDBString(50, IsUnicode = true)]
		public virtual string TypeName { get; set; }
		#endregion
		#region SettingName
		public abstract class settingName : PX.Data.BQL.BqlString.Field<settingName> { }
		[PXDBString(255, IsKey = true, IsUnicode = true)]
		[PXDefault]
		public virtual string SettingName { get; set; }
		#endregion
		#region AdditionalInformation
		[PXDBString(2048, IsUnicode = true)]
		public virtual string AdditionalInformation { get; set; }
		public abstract class additionalInformation : PX.Data.BQL.BqlString.Field<additionalInformation> { }
		#endregion
		#region UsedForTaxCalculation
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? UsedForTaxCalculation { get; set; }
		public abstract class usedForTaxCalculation : PX.Data.BQL.BqlBool.Field<usedForTaxCalculation> { }
		#endregion
		#region FormBox
		[PXDBString(255, IsUnicode = true)]
		public virtual string FormBox { get; set; }
		public abstract class formBox : PX.Data.BQL.BqlString.Field<formBox> { }
		#endregion
		#region State
		[PXDBString(3, IsUnicode = true, IsKey = true)]
		[PXDefault(TaxSettingAdditionalInformationKey.StateFallback)]
		public virtual string State { get; set; }
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		#endregion
		#region CountryID
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault]
		public virtual string CountryID { get; set; }
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
