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

using PX.Objects.Common;
using PX.Objects.IN;
using PX.SM;
using PX.SM.Email;

namespace PX.Objects.CR
{
	using System;
	using System.Linq;
	using PX.Data;
	using PX.Data.BQL.Fluent;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CS;
	using PX.Objects.CT;

	[PXCacheName(Messages.CaseClass)]
	[PXPrimaryGraph(typeof(CRCaseClassMaint))]
	[System.SerializableAttribute()]
	public partial class CRCaseClass : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<CRCaseClass>.By<caseClassID>
		{
			public static CRCaseClass Find(PXGraph graph, string caseClassID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, caseClassID, options);
		}
		public static class FK
		{
			public class DefaultEmailAccount : PX.SM.EMailAccount.PK.ForeignKeyOf<CRCaseClass>.By<defaultEMailAccountID> { }

			public class LabourItem : IN.InventoryItem.PK.ForeignKeyOf<CRCaseClass>.By<labourItemID> { }
			public class OvertimeItem : IN.InventoryItem.PK.ForeignKeyOf<CRCaseClass>.By<overtimeItemID> { }
		}
		#endregion

		#region CaseClassID
		public abstract class caseClassID : PX.Data.BQL.BqlString.Field<caseClassID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Case Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(CRCaseClass.caseClassID), DescriptionField = typeof(CRCaseClass.description))]
		public virtual String CaseClassID { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		[PXDBLocalizableString(255, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description { get; set; }
		#endregion

		#region IsBillable
		public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Billable")]
		[PXUIEnabled(typeof(Where<
			perItemBilling.IsEqual<BillingTypeListAttribute.perCase>>))]
		[UndefaultFormula(typeof(Switch<
			Case<
				Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>,
				False,
			Case<
				Where<Editable<requireCustomer>, Equal<True>, And<allowEmployeeAsContact, Equal<True>>>,
				False>>,
			isBillable>))]
		public virtual Boolean? IsBillable { get; set; }
		#endregion

		#region AllowOverrideBillable
		public abstract class allowOverrideBillable : PX.Data.BQL.BqlBool.Field<allowOverrideBillable> { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Billable Option Override")]
		[PXUIEnabled(typeof(Where<
			perItemBilling.IsEqual<BillingTypeListAttribute.perCase>>))]
		[UndefaultFormula(typeof(Switch<
			Case<
				Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>,
				False,
			Case<
				Where<Editable<requireCustomer>, Equal<True>, And<allowEmployeeAsContact, Equal<True>>>,
				False>>,
			allowOverrideBillable>))]
		public virtual Boolean? AllowOverrideBillable { get; set; }
		#endregion

		#region RequireCustomer
		public abstract class requireCustomer : PX.Data.BQL.BqlBool.Field<requireCustomer> { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Customer")]
		[PXUIEnabled(typeof(Where<
			isBillable.IsNotEqual<True>
			.Or<allowEmployeeAsContact.IsEqual<True>>>))]
		[UndefaultFormula(typeof(Switch<
			Case<
				Where<Editable<requireCustomer>, Equal<True>, And<allowEmployeeAsContact, Equal<True>>>,
				False,
			Case<
				Where<isBillable, Equal<True>, Or<allowOverrideBillable, Equal<True>>>,
				True>>,
			requireCustomer>))]
		public virtual Boolean? RequireCustomer { get; set; }
		#endregion

		#region RequireContact
		public abstract class requireContact : PX.Data.BQL.BqlBool.Field<requireContact> { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Contact")]
		public virtual Boolean? RequireContact { get; set; }
		#endregion

		#region AllowEmployeeAsContact
		public abstract class allowEmployeeAsContact : PX.Data.BQL.BqlBool.Field<allowEmployeeAsContact> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Selecting Employee as Case Contact")]
		[UndefaultFormula(typeof(Switch<
			Case<
				Where<Editable<requireCustomer>, Equal<False>, And<allowEmployeeAsContact, Equal<True>>>,
				False>,
			allowEmployeeAsContact>))]
		public virtual Boolean? AllowEmployeeAsContact { get; set; }
		#endregion

		#region RequireContract
		public abstract class requireContract : PX.Data.BQL.BqlBool.Field<requireContract> { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, True>, Current<requireContract>>))]
		[PXUIEnabled(typeof(Where<perItemBilling, NotEqual<BillingTypeListAttribute.perActivity>>))]
		[PXUIField(DisplayName = "Require Contract")]
		public virtual Boolean? RequireContract { get; set; }
		#endregion

		#region PerItemBilling
		public abstract class perItemBilling : PX.Data.BQL.BqlInt.Field<perItemBilling> { }

		[PXDBInt()]
		[BillingTypeList()]
		[PXDefault(BillingTypeListAttribute.PerCase)]
		[PXUIField(DisplayName = "Billing Mode")]
		public virtual Int32? PerItemBilling { get; set; }
		#endregion

		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }
        protected Int32? _LabourItemID;
        [PXDBInt()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor Item", Required = false)]
        [PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		[PXUIRequired(typeof(Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>, And<Where<isBillable, Equal<True>, Or<allowOverrideBillable, Equal<True>>>>>))]
		[PXUIEnabled(typeof(Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>))]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, Null>, Current<labourItemID>>))]
		public virtual Int32? LabourItemID
        {
            get
            {
                return this._LabourItemID;
            }
            set
            {
                this._LabourItemID = value;
            }
        }
        #endregion

        #region OvertimeItemID
        public abstract class overtimeItemID : PX.Data.BQL.BqlInt.Field<overtimeItemID> { }
        protected Int32? _OvertimeItemID;
        [PXDBInt()]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Overtime Labor Item", Required = false)]
        [PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.itemType, Equal<INItemTypes.laborItem>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXForeignReference(typeof(Field<overtimeItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		[PXUIRequired(typeof(Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>, And<Where<isBillable, Equal<True>, Or<allowOverrideBillable, Equal<True>>>>>))]
		[PXUIEnabled(typeof(Where<perItemBilling, Equal<BillingTypeListAttribute.perCase>>))]
		[PXFormula(typeof(Switch<Case<Where<perItemBilling, Equal<BillingTypeListAttribute.perActivity>>, Null>, Current<overtimeItemID>>))]
		public virtual Int32? OvertimeItemID
        {
            get
            {
                return this._OvertimeItemID;
            }
            set
            {
                this._OvertimeItemID = value;
            }
        }
        #endregion

		#region DefaultEMailAccount
		public abstract class defaultEMailAccountID : PX.Data.BQL.BqlInt.Field<defaultEMailAccountID>
		{
			// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
			public class EmailAccountRule
				: EMailAccount.userID.PreventMakingPersonalIfUsedAsSystem<
					SelectFrom<CRCaseClass>.Where<FK.DefaultEmailAccount.SameAsCurrent>> {}
		}

		[EmailAccountRaw(emailAccountsToShow: EmailAccountsToShowOptions.OnlySystem, DisplayName = "Default Email Account")]
		[PXForeignReference(typeof(FK.DefaultEmailAccount), Data.ReferentialIntegrity.ReferenceBehavior.SetNull)]
		public virtual int? DefaultEMailAccountID { get; set; }
		#endregion

		#region RoundingInMinutes
		public abstract class roundingInMinutes : PX.Data.BQL.BqlInt.Field<roundingInMinutes> { }
		protected Int32? _RoundingInMinutes;
		[PXDBInt()]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Round Time by")]
		[PXTimeList(5, 6)]
		public virtual Int32? RoundingInMinutes
		{
			get
			{
				return this._RoundingInMinutes;
			}
			set
			{
				this._RoundingInMinutes = value;
			}
		}
		#endregion

		#region MinBillTimeInMinutes
		public abstract class minBillTimeInMinutes : PX.Data.BQL.BqlInt.Field<minBillTimeInMinutes> { }
		protected Int32? _MinBillTimeInMinutes;
		[PXDBInt()]
		[PXDefault(0)]
		[PXTimeList(5, 12)]
		[PXUIField(DisplayName = "Min Billable Time", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? MinBillTimeInMinutes
		{
			get
			{
				return this._MinBillTimeInMinutes;
			}
			set
			{
				this._MinBillTimeInMinutes = value;
			}
		}
		#endregion

		#region ReopenCaseTimeInDays
		public abstract class reopenCaseTimeInDays : PX.Data.BQL.BqlInt.Field<reopenCaseTimeInDays> { }
		protected Int32? _ReopenCaseTimeInDays;
		[PXDBInt]
		[PXUIField(DisplayName = "Allowed Period to Reopen Case (in Days)")]
		public virtual Int32? ReopenCaseTimeInDays
		{
			get
			{
				return this._ReopenCaseTimeInDays;
			}
			set
			{
				this._ReopenCaseTimeInDays = value;
			}
		}
		#endregion

		#region IsInternal

		public abstract class isInternal : PX.Data.BQL.BqlBool.Field<isInternal> { }
		protected Boolean? _IsInternal;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Internal", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? IsInternal
		{
			get
			{
				return this._IsInternal;
			}
			set
			{
				this._IsInternal = value;
			}
		}

		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion

		}

	public class BillingTypeListAttribute : PXIntListAttribute
	{
		public const int PerCase = 0;
		public const int PerActivity = 1;
        
		public class perCase : PX.Data.BQL.BqlInt.Constant<perCase>
		{
			public perCase() : base(PerCase) { }
		}

		public class perActivity : PX.Data.BQL.BqlInt.Constant<perActivity>
		{
			public perActivity() : base(PerActivity) { }
		}

        public override void CacheAttached(PXCache sender)
        {
            _AllowedValues = PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>()
                ? new[] { PerCase, PerActivity }
                : new[] { PerCase };

            _AllowedLabels = PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>()
                ? new[] { Messages.PerCase, Messages.PerActivity }
                : new[] { Messages.PerCase };

			_NeutralAllowedLabels = _AllowedLabels;

			base.CacheAttached(sender);
        }
	}
}
