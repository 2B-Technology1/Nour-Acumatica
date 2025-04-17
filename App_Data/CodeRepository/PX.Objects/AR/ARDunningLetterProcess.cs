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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.SM;
using PX.Data.BQL;
using static PX.Objects.Common.Extensions.CollectionExtensions;
using PX.Objects.Common;

namespace PX.Objects.AR
{
	public class ARDunningLetterProcess : PXGraph<ARDunningLetterProcess>
	{
		#region internal types definition
		[Serializable]
		[PXProjection(typeof(Select5<Standalone.ARInvoice,
					InnerJoin<ARRegister,
						On<Standalone.ARInvoice.docType, Equal<ARRegister.docType>,
						And<Standalone.ARInvoice.refNbr, Equal<ARRegister.refNbr>,
						And<ARRegister.released, Equal<True>,
						And<ARRegister.openDoc, Equal<True>,
						And<ARRegister.voided, Equal<False>,
						And<ARRegister.pendingPPD, NotEqual<True>,
						And<Where<ARRegister.docType, Equal<ARDocType.invoice>,
							Or<ARRegister.docType, Equal<ARDocType.finCharge>,
							Or<ARRegister.docType, Equal<ARDocType.debitMemo>>>>>>>>>>>,
					InnerJoin<Customer, 
						On<Customer.bAccountID, Equal<ARRegister.customerID>>,
					LeftJoin<ARDunningLetterDetail,
						On<ARDunningLetterDetail.dunningLetterBAccountID, Equal<Customer.sharedCreditCustomerID>,
						And<ARDunningLetterDetail.docType, Equal<ARRegister.docType>,
						And<ARDunningLetterDetail.refNbr, Equal<ARRegister.refNbr>,
						And<ARDunningLetterDetail.voided, Equal<False>>>>>,
					LeftJoin<ARDunningLetter,
						On<ARDunningLetter.dunningLetterID, Equal<ARDunningLetterDetail.dunningLetterID>,
						And<ARDunningLetter.voided, Equal<False>>>>>>>,
					Aggregate<
						GroupBy<ARRegister.refNbr,
						GroupBy<ARRegister.docType,
						Min<ARDunningLetterDetail.released>>>>>))]
		public partial class ARInvoiceWithDL : IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			[PXDBInt(BqlField = typeof(ARRegister.customerID))]
			public virtual int? CustomerID
			{
				get;
				set;
			}
			#endregion
			#region SharedCreditCustomerID
			public abstract class sharedCreditCustomerID : PX.Data.BQL.BqlInt.Field<sharedCreditCustomerID> { }

			[PXDBInt(BqlField = typeof(Customer.sharedCreditCustomerID))]
			public virtual int? SharedCreditCustomerID
			{
				get;
				set;
			}
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			[PXDBInt(BqlField = typeof(ARRegister.branchID))]
			public virtual int? BranchID
			{
				get;
				set;
			}
			#endregion
			#region CustomerLocationID
			public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
		
			[PXDBInt(BqlField = typeof(ARRegister.customerLocationID))]
			public virtual Int32? CustomerLocationID
			{
				get;
				set;
			}
			#endregion
			#region DocBal
			public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
			[PXDBDecimal(BqlField = typeof(ARRegister.docBal))]
			public virtual decimal? DocBal
			{
				get;
				set;
			}
			#endregion
			#region DueDate
			public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
			[PXDBDate(BqlField = typeof(ARRegister.dueDate))]
			public virtual DateTime? DueDate
			{
				get;
				set;
			}
			#endregion
			#region Released
			public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
			[PXDBBool(BqlField = typeof(ARRegister.released))]
			public virtual bool? Released
			{
				get;
				set;
			}
			#endregion
			#region OpenDoc
			public abstract class openDoc : PX.Data.BQL.BqlBool.Field<openDoc> { }
			[PXDBBool(BqlField = typeof(ARRegister.openDoc))]
			public virtual bool? OpenDoc
			{
				get;
				set;
			}
			#endregion
			#region Voided
			public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
			[PXDBBool(BqlField = typeof(ARRegister.voided))]
			public virtual bool? Voided
			{
				get;
				set;
			}
			#endregion
			#region Revoked
			public abstract class revoked : PX.Data.BQL.BqlBool.Field<revoked> { }
			[PXDBBool(BqlField = typeof(AR.Standalone.ARInvoice.revoked))]
			public virtual bool? Revoked
			{
				get;
				set;
			}
			#endregion
			#region DocType
			public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
			[PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(ARRegister.docType))]
			public virtual string DocType
			{
				get;
				set;
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
			[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(ARRegister.refNbr))]
			public virtual string RefNbr
			{
				get;
				set;
			}
			#endregion
			#region DunningLetterLevel
			public abstract class dunningLetterLevel : PX.Data.BQL.BqlInt.Field<dunningLetterLevel> { }
			[PXDBInt(BqlField = typeof(ARDunningLetterDetail.dunningLetterLevel))]
			public virtual int? DunningLetterLevel
			{
				get;
				set;
			}
			#endregion
			#region DunningLetterDate
			public abstract class dunningLetterDate : PX.Data.BQL.BqlDateTime.Field<dunningLetterDate> { }
			[PXDBDate(BqlField = typeof(ARDunningLetter.dunningLetterDate))]
			public virtual DateTime? DunningLetterDate
			{
				get;
				set;
			}
			#endregion
			#region DocDate
			public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
			[PXDBDate(BqlField = typeof(ARRegister.docDate))]
			public virtual DateTime? DocDate
			{
				get;
				set;
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			[PXDBString(5, IsUnicode = true, BqlField = typeof(ARRegister.curyID))]
			public virtual string CuryID
			{
				get;
				set;
			}
			#endregion
			#region CuryOrigDocAmt
			public abstract class curyOrigDocAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDocAmt> { }
			[PXDBDecimal(BqlField = typeof(ARRegister.curyOrigDocAmt))]
			public virtual decimal? CuryOrigDocAmt
			{
				get;
				set;
			}
			#endregion
			#region OrigDocAmt
			public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
			[PXDBDecimal(BqlField = typeof(ARRegister.origDocAmt))]
			public virtual decimal? OrigDocAmt
			{
				get;
				set;
			}
			#endregion
			#region CuryDocBal
			public abstract class curyDocBal : PX.Data.BQL.BqlDecimal.Field<curyDocBal> { }
			[PXDBDecimal(BqlField = typeof(ARRegister.curyDocBal))]
			public virtual decimal? CuryDocBal
			{
				get;
				set;
			}
			#endregion
			#region DLReleased
			public abstract class dLReleased : PX.Data.BQL.BqlBool.Field<dLReleased> { }
			[PXDBBool(BqlField = typeof(ARDunningLetterDetail.released))]
			public virtual bool? DLReleased
			{
				get;
				set;
			}
			#endregion
        }

		protected class IncludeTypes
		{
			public const int IncludeAll = 0;
			public const int IncludeLevels = 1;

			public class ListAttribute : PXIntListAttribute
			{
				public ListAttribute() :
					base(new int[] { IncludeAll, IncludeLevels },
						new string[] { Messages.IncludeAllToDL, Messages.IncludeLevelsToDL })
				{ }
			}
		}
		[Serializable]
		public partial class ARDunningLetterRecordsParameters : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
			[Organization(onlyActive: true, Required = false)]
			public int? OrganizationID { get; set; }
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			[BranchOfOrganization(typeof(organizationID), onlyActive: true, Required = false)]
			public int? BranchID { get; set; }
			#endregion
			#region OrgBAccountID
			public abstract class orgBAccountID : IBqlField { }
			[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: true)]
			public int? OrgBAccountID { get; set; }
			#endregion

			#region CustomerClass
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Customer Class", Visibility = PXUIVisibility.Visible)]
			[PXSelector(typeof(CustomerClass.customerClassID))]
			public virtual string CustomerClassID
			{
				get;
				set;
			}
			#endregion
			#region DocDate
			public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
			[PXDate]
			[PXDefault(typeof(AccessInfo.businessDate))]
			[PXUIField(DisplayName = "Dunning Letter Date", Visibility = PXUIVisibility.Visible, Required = true)]
			public virtual DateTime? DocDate
			{
				get;
				set;
			}
			#endregion

			#region IncludeNonOverdueDunning
			public abstract class includeNonOverdueDunning : PX.Data.BQL.BqlBool.Field<includeNonOverdueDunning> { }
			[PXDBBool]
			[PXDefault(typeof(Search<ARSetup.includeNonOverdueDunning>))]
			[PXUIField(DisplayName = Messages.IncludeNonOverdue, Visibility = PXUIVisibility.Visible)]
			public virtual bool? IncludeNonOverdueDunning
			{
				get;
				set;
			}
			#endregion
			#region AddOpenPaymentsAndCreditMemos
			public abstract class addOpenPaymentsAndCreditMemos : PX.Data.BQL.BqlBool.Field<addOpenPaymentsAndCreditMemos> { }
			[PXDBBool]
			[PXDefault(typeof(Search<ARSetup.addOpenPaymentsAndCreditMemos>))]
			[PXUIField(DisplayName = Messages.AddOpenPaymentsAndCreditMemos, Visibility = PXUIVisibility.Visible)]
			public virtual bool? AddOpenPaymentsAndCreditMemos
			{
				get;
				set;
			}
			#endregion
			#region IncludeType
			public abstract class includeType : PX.Data.BQL.BqlInt.Field<includeType> { }
			[PXInt]
			[PXDefault(IncludeTypes.IncludeAll)]
			[PXUIField(DisplayName = "Include Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
			[IncludeTypes.List]
			public virtual int? IncludeType
			{
				get;
				set;
			}
			#endregion
			#region LevelFrom
			public abstract class levelFrom : PX.Data.BQL.BqlInt.Field<levelFrom> { }
			[PXInt]
			[PXDefault(1, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "From", Enabled = false)]
			public virtual int? LevelFrom
			{
				get;
				set;
			}
			#endregion
			#region LevelTo
			public abstract class levelTo : PX.Data.BQL.BqlInt.Field<levelTo> { }
			[PXInt]
			[PXDefault(typeof(Search<ARDunningCustomerClass.dunningLetterLevel, Where<True, Equal<True>>, OrderBy<Desc<ARDunningCustomerClass.dunningLetterLevel>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "To", Enabled = false)]
			public virtual int? LevelTo
			{
				get;
				set;
			}
			#endregion
		}

		[Serializable]
		public partial class ARDunningLetterList : IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
			[PXBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Selected")]
			public virtual bool? Selected
			{
				get;
				set;
			}
			#endregion
			#region CustomerClass
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Customer Class", Visibility = PXUIVisibility.Visible)]
			public virtual string CustomerClassID
			{
				get;
				set;
			}
			#endregion
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
			[Organization(IsKey = true)]
			public int? OrganizationID { get; set; }
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

			[Branch(IsKey = true)]
			public virtual int? BranchID
			{
				get;
				set;
			}
			#endregion
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[PXDBInt(IsKey = true)]
			[PXDefault]
			[Customer(DescriptionField = typeof(Customer.acctName))]
			[PXUIField(DisplayName = "Customer")]
			public virtual int? BAccountID
			{
				get;
				set;
			}
			#endregion

			#region DueDate
			public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }
			[PXDBDate(IsKey = true)]
			[PXDefault]
			[PXUIField(DisplayName = "Earliest Due Date")]
			public virtual DateTime? DueDate
			{
				get;
				set;
			}
			#endregion
			#region NumberOfDocuments
			public abstract class numberOfDocuments : PX.Data.BQL.BqlInt.Field<numberOfDocuments> { }
			[PXInt]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Number of Documents")]
			public virtual int? NumberOfDocuments
			{
				get;
				set;
			}
			#endregion
			#region NumberOfOverdueDocuments
			public abstract class numberOfOverdueDocuments : PX.Data.BQL.BqlInt.Field<numberOfOverdueDocuments> { }
			[PXInt]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Number of Overdue Documents")]
			public virtual int? NumberOfOverdueDocuments
			{
				get;
				set;
			}
			#endregion
			#region OrigDocAmt
			public abstract class origDocAmt : PX.Data.BQL.BqlDecimal.Field<origDocAmt> { }
			[PXDBBaseCuryMaxPrecision]
			[PXUIField(DisplayName = "Customer Balance")]
			public virtual decimal? OrigDocAmt
			{
				get;
				set;
			}
			#endregion
			#region DocBal
			public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
			[PXDBBaseCuryMaxPrecision]
			[PXUIField(DisplayName = "Overdue Balance")]
			public virtual decimal? DocBal
			{
				get;
				set;
			}
			#endregion

			#region DunningLetterLevel
			public abstract class dunningLetterLevel : PX.Data.BQL.BqlInt.Field<dunningLetterLevel> { }
			[PXInt]
			[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Dunning Letter Level")]
			public virtual int? DunningLetterLevel
			{
				get;
				set;
			}
			#endregion
			#region LastDunningLetterDate
			public abstract class lastDunningLetterDate : PX.Data.BQL.BqlDateTime.Field<lastDunningLetterDate> { }
			[PXDBDate(IsKey = true)]
			[PXDefault]
			[PXUIField(DisplayName = "Last Dunning Letter Date")]
			public virtual DateTime? LastDunningLetterDate
			{
				get;
				set;
			}
			#endregion
			#region DueDays
			public abstract class dueDays : PX.Data.BQL.BqlInt.Field<dueDays> { }
			[PXDBInt]
			[PXDefault]
			[PXUIField(DisplayName = "Due Days")]
			public virtual int? DueDays
			{
				get;
				set;
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

			[PXDBString(5, IsUnicode = true)]
			[PXUIField(DisplayName = "Currency")]
			[PXSelector(typeof(Currency.curyID))]
			public virtual string CuryID { get; set; }
			#endregion
		}

		#endregion

		#region selects+ctor
		public PXFilter<ARDunningLetterRecordsParameters> Filter;
		public PXCancel<ARDunningLetterRecordsParameters> Cancel;

		[PXFilterable]
		[PXVirtualDAC]
		public PXFilteredProcessing<ARDunningLetterList, ARDunningLetterRecordsParameters> DunningLetterList;

		public PXSetup<ARSetup> arsetup;

		private readonly Dictionary<string, int> MaxDunningLevels = new Dictionary<string, int>();

		private readonly int DunningLetterProcessTypeARSetup;

		[PXViewName("DunningLetter")]
		public PXSelect<ARDunningLetter> docs;
		[PXViewName("DunningLetterDetail")]
		public PXSelect<ARDunningLetterDetail,
			Where<ARDunningLetterDetail.dunningLetterID, Equal<Required<ARDunningLetter.dunningLetterID>>>> docsDet;

		public ARDunningLetterProcess()
		{
			DunningLetterList.Cache.AllowDelete = false;
			DunningLetterList.Cache.AllowInsert = false;
			DunningLetterList.Cache.AllowUpdate = true;

			// Acuminator disable once PX1085 DatabaseQueriesInPXGraphInitialization [It was the same before the changes.]
			DunningLetterProcessTypeARSetup = ((ARSetup)arsetup.Select()).DunningLetterProcessType.Value;
			bool processByCustomer = DunningLetterProcessTypeARSetup == DunningProcessType.ProcessByCustomer;
			var arPrefs = (ARSetup)arsetup.Select();
			bool hasDunningFeeInventoryID = arsetup.Current.DunningFeeInventoryID.HasValue;

			PXUIFieldAttribute.SetVisible<ARDunningLetterRecordsParameters.includeType>(Filter.Cache, null, !processByCustomer);
			PXUIFieldAttribute.SetVisible<ARDunningLetterRecordsParameters.levelFrom>(Filter.Cache, null, !processByCustomer);
			PXUIFieldAttribute.SetVisible<ARDunningLetterRecordsParameters.levelTo>(Filter.Cache, null, !processByCustomer);

			// Acuminator disable once PX1085 DatabaseQueriesInPXGraphInitialization [Request must be executed only once. It was the same before the changes.]
			foreach (ARDunningCustomerClass setup in SelectFrom<ARDunningCustomerClass>
					.AggregateTo<GroupBy<ARDunningCustomerClass.customerClassID>,
						Max<ARDunningCustomerClass.dunningLetterLevel>>
					.View.SelectMultiBound(this, null, null))
			{
				this.MaxDunningLevels.Add(setup.CustomerClassID, setup.DunningLetterLevel ?? 0);
			}

			DunningLetterList.SetProcessDelegate(list => DunningLetterProc(list, Filter.Current));
			PXUIFieldAttribute.SetEnabled(DunningLetterList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARDunningLetterList.selected>(DunningLetterList.Cache, null, true);
			DunningLetterList.SetSelected<ARDunningLetterList.selected>();

			PXUIFieldAttribute.SetVisible<ARDunningLetterRecordsParameters.orgBAccountID>(Filter.Cache, null, arPrefs.PrepareDunningLetters == ARSetup.prepareDunningLetters.ForEachBranch);
			PXUIFieldAttribute.SetVisible<ARDunningLetterRecordsParameters.organizationID>(Filter.Cache, null, arPrefs.PrepareDunningLetters == ARSetup.prepareDunningLetters.ConsolidatedForCompany);
			PXUIFieldAttribute.SetEnabled<ARDunningLetterRecordsParameters.organizationID>(Filter.Cache, null, arPrefs.PrepareDunningLetters == ARSetup.prepareDunningLetters.ConsolidatedForCompany);
		}
		#endregion

		#region events
		protected virtual void ARDunningLetterRecordsParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARDunningLetterRecordsParameters row = (ARDunningLetterRecordsParameters)e.Row;
			if (row != null)
			{
				ARDunningLetterRecordsParameters filter = (ARDunningLetterRecordsParameters)this.Filter.Cache.CreateCopy(row);
				DunningLetterList.SetProcessDelegate(list => DunningLetterProc(list, filter));

				bool includeAll = row.IncludeType == IncludeTypes.IncludeAll;
				PXUIFieldAttribute.SetEnabled<ARDunningLetterRecordsParameters.levelFrom>(sender, null, !includeAll);
				PXUIFieldAttribute.SetEnabled<ARDunningLetterRecordsParameters.levelTo>(sender, null, !includeAll);
				PXUIFieldAttribute.SetVisible<ARDunningLetterRecordsParameters.levelFrom>(sender, null, !includeAll);
				PXUIFieldAttribute.SetVisible<ARDunningLetterRecordsParameters.levelTo>(sender, null, !includeAll);
			}
		}

		protected virtual void ARDunningLetterRecordsParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			DunningLetterList.Cache.Clear();
		}

		protected virtual void _(Events.RowUpdating<ARDunningLetterRecordsParameters> e)
		{
			if (DunningLetterProcessTypeARSetup == DunningProcessType.ProcessByCustomer
				&& arsetup.Current.DunningFeeInventoryID.HasValue
				&& DunningLetterList.Cache.IsDirty)
			{
				e.Cancel = true;

				if (DunningLetterList.Ask(Messages.Warning, Messages.ChangesInTheDunningFee, MessageButtons.YesNo) == WebDialogResult.Yes)
				{
					e.Cancel = false;
				}
			}
		}


		protected virtual void _(Events.FieldUpdated<ARDunningLetterRecordsParameters, ARDunningLetterRecordsParameters.customerClassID> e)
		{
			BqlCommand cmd = new SelectFrom<ARDunningCustomerClass>.AggregateTo<Max<ARDunningCustomerClass.dunningLetterLevel>>();

			if (e.NewValue != null)
			{
				cmd = cmd.WhereAnd<Where<ARDunningCustomerClass.customerClassID.IsEqual<@P.AsString>>>();
			}

			ARDunningCustomerClass d = cmd.Select<ARDunningCustomerClass>(this, e.NewValue).FirstOrDefault();
			e.Row.LevelTo = d.DunningLetterLevel;
		}
		#endregion

		#region Delegate for select
		protected virtual IEnumerable dunningLetterList()
		{
			ARDunningLetterRecordsParameters header = Filter.Current;
			if (header == null || header.DocDate == null)
			{
				yield break;
			}
			IEnumerable<ARDunningLetterList> result = PrepareList();

			foreach (ARDunningLetterList item in result)
			{
				item.Selected = (DunningLetterList.Locate(item) ?? item).Selected;
				DunningLetterList.Cache.Hold(item);
				yield return item;
			}
		}

		private IEnumerable<ARDunningLetterList> PrepareList()
		{
			IEnumerable<PXResult<Customer>> rows = GetData();
			return ComposeResult(rows);
		}

		protected virtual IEnumerable<PXResult<Customer>> GetData()
		{
			ARSetup arPrefs = arsetup.Current;
			bool consolidateByBranch = arPrefs.PrepareDunningLetters == ARSetup.prepareDunningLetters.ForEachBranch;
			bool consolidateByCompany = arPrefs.PrepareDunningLetters == ARSetup.prepareDunningLetters.ConsolidatedForCompany;

			ARDunningLetterRecordsParameters header = Filter.Current; 
			if (DunningLetterProcessTypeARSetup == DunningProcessType.ProcessByCustomer)
			{
				var cmd = new PXSelectJoinGroupBy<Customer,
					InnerJoin<ARInvoiceWithDL,
						On<ARInvoiceWithDL.customerID, Equal<Customer.bAccountID>,
						And<ARInvoiceWithDL.dueDate, Less<Required<ARDunningLetterRecordsParameters.docDate>>>>,
					InnerJoin<CustomerAlias,
						On<Customer.sharedCreditCustomerID, Equal<CustomerAlias.bAccountID>>,
					LeftJoin<ARDunningCustomerClass,
						On<CustomerAlias.customerClassID, Equal<ARDunningCustomerClass.customerClassID>,
						And<Where<ARInvoiceWithDL.dunningLetterLevel.Add<int1>, Equal<ARDunningCustomerClass.dunningLetterLevel>,
							Or<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull, And<ARDunningCustomerClass.dunningLetterLevel, Equal<int1>>>>>>>,
					LeftJoin<ARDunningCustomerClassAlias,
						On<CustomerAlias.customerClassID, Equal<ARDunningCustomerClassAlias.customerClassID>,
							And<ARInvoiceWithDL.dunningLetterLevel, Equal<ARDunningCustomerClassAlias.dunningLetterLevel>>>,
					LeftJoin<ARBalances,
						On<ARBalances.customerID, Equal<Customer.sharedCreditCustomerID>,
						And<ARBalances.branchID, Equal<ARInvoiceWithDL.branchID>,
						And<ARBalances.customerLocationID, Equal<ARInvoiceWithDL.customerLocationID>>>>>>>>>,
					Where2<Where<Customer.printDunningLetters, Equal<True>,
							Or<Customer.mailDunningLetters, Equal<True>>>,
							And2<Where<ARInvoiceWithDL.dLReleased, Equal<True>,
							Or<ARInvoiceWithDL.dLReleased, IsNull>>,
						And2<Match<Current<AccessInfo.userName>>,
						And2<Where<Customer.customerClassID, Equal<Required<ARDunningLetterRecordsParameters.customerClassID>>,
							Or<Required<ARDunningLetterRecordsParameters.customerClassID>, IsNull>>,
						And<Where<ARInvoiceWithDL.dunningLetterLevel, IsNotNull,
							Or<ARDunningCustomerClass.dunningLetterLevel, IsNotNull>>>>>>>,
					Aggregate<GroupBy<Customer.sharedCreditCustomerID,
						GroupBy<ARInvoiceWithDL.branchID,
						Min<ARInvoiceWithDL.dueDate,
						Sum<ARInvoiceWithDL.docBal,
						Count<ARInvoiceWithDL.refNbr>>>>>>>(this);

				List<object> par = new List<object> { header.DocDate, header.CustomerClassID, header.CustomerClassID };

				if (consolidateByBranch && header.OrgBAccountID != null)
				{
					cmd.WhereAnd<Where<ARInvoiceWithDL.branchID,
						Inside<Required<ARDunningLetterRecordsParameters.orgBAccountID>>>>();
					par.Add(header.OrgBAccountID);
				}
				else if (consolidateByCompany && header.OrganizationID != null)
				{
					var organization = PXAccess.GetOrganizationByID(header.OrganizationID);
					header.OrgBAccountID = organization.BAccountID;

					cmd.WhereAnd<Where<ARInvoiceWithDL.branchID,
						Inside<Required<ARDunningLetterRecordsParameters.orgBAccountID>>>>();
					par.Add(organization.BAccountID);
				}

				List<PXResult<Customer>> results = cmd.Select(par.ToArray()).ToList();
				return results;

			}
			else if (DunningLetterProcessTypeARSetup == DunningProcessType.ProcessByDocument)
			{
				var cmd = new PXSelectJoinGroupBy<Customer,
					InnerJoin<ARInvoiceWithDL,
						On<ARInvoiceWithDL.customerID, Equal<Customer.bAccountID>,
						And<ARInvoiceWithDL.revoked, Equal<False>>>,
					InnerJoin<CustomerAlias,
						On<Customer.sharedCreditCustomerID, Equal<CustomerAlias.bAccountID>>,
					LeftJoin<ARDunningCustomerClass,
						On<CustomerAlias.customerClassID, Equal<ARDunningCustomerClass.customerClassID>,
						And<Where<ARInvoiceWithDL.dunningLetterLevel.Add<int1>, Equal<ARDunningCustomerClass.dunningLetterLevel>,
							Or<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull, And<ARDunningCustomerClass.dunningLetterLevel, Equal<int1>>>>>>>,
					LeftJoin<ARDunningCustomerClassAlias,
						On<CustomerAlias.customerClassID, Equal<ARDunningCustomerClassAlias.customerClassID>,
							And<ARInvoiceWithDL.dunningLetterLevel, Equal<ARDunningCustomerClassAlias.dunningLetterLevel>>>,
					LeftJoin<ARBalances,
						On<ARBalances.customerID, Equal<Customer.sharedCreditCustomerID>,
						And<ARBalances.branchID, Equal<ARInvoiceWithDL.branchID>,
						And<ARBalances.customerLocationID, Equal<ARInvoiceWithDL.customerLocationID>>>>>>>>>,
					Where2<Where<Customer.printDunningLetters, Equal<True>,
							Or<Customer.mailDunningLetters, Equal<True>>>,
						And2<Where<ARInvoiceWithDL.dLReleased, Equal<True>,
							Or<ARInvoiceWithDL.dLReleased, IsNull>>,
						And2<Match<Current<AccessInfo.userName>>,
						And<Add<ARInvoiceWithDL.dueDate, ARDunningCustomerClass.dueDays>, Less<Required<ARDunningLetterRecordsParameters.docDate>>,
						And<Where<ARInvoiceWithDL.dunningLetterLevel, IsNotNull,
							Or<ARDunningCustomerClass.dunningLetterLevel, IsNotNull>>>>>>>,
					Aggregate<GroupBy<ARInvoiceWithDL.dunningLetterLevel,
						GroupBy<Customer.sharedCreditCustomerID,
						GroupBy<ARInvoiceWithDL.branchID,
						Min<ARInvoiceWithDL.dueDate,
						Sum<ARInvoiceWithDL.docBal,
						Count<ARInvoiceWithDL.refNbr>>>>>>>>(this);

				List<object> par = new List<object> { header.DocDate };

				if (consolidateByBranch && header.OrgBAccountID != null)
				{
					cmd.WhereAnd<Where<ARInvoiceWithDL.branchID,
						Inside<Required<ARDunningLetterRecordsParameters.orgBAccountID>>>>();
					par.Add(header.OrgBAccountID);
				}
				else if (consolidateByCompany && header.OrganizationID != null)
				{
					var organization = PXAccess.GetOrganizationByID(header.OrganizationID);
					header.OrgBAccountID = organization.BAccountID;

					cmd.WhereAnd<Where<ARInvoiceWithDL.branchID,
						Inside<Required<ARDunningLetterRecordsParameters.orgBAccountID>>>>();
					par.Add(organization?.BAccountID);
				}

				if (header.CustomerClassID != null)
				{
					cmd.WhereAnd<Where<Customer.customerClassID, Equal<Required<ARDunningLetterRecordsParameters.customerClassID>>>>();
					par.Add(header.CustomerClassID);
				}

				if (header.IncludeType == 1)
				{
					cmd.WhereAnd<Where2<Where<ARInvoiceWithDL.dunningLetterLevel, GreaterEqual<Required<ARDunningLetterRecordsParameters.levelFrom>>,
										And<ARInvoiceWithDL.dunningLetterLevel, LessEqual<Required<ARDunningLetterRecordsParameters.levelTo>>>>,
					Or<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull, And<Required<ARDunningLetterRecordsParameters.levelFrom>, Less<int1>>>>>>();

					par.Add(header.LevelFrom - 1);
					par.Add(header.LevelTo - 1);
					par.Add(header.LevelFrom - 1);
				}

				List<PXResult<Customer>> results = new List<PXResult<Customer>>();

				results = cmd.Select(par.ToArray()).ToList();
				return results;
			}
			else
			{
				throw new NotImplementedException();
			}
		}



		protected virtual List<ARDunningLetterList> ComposeResult(IEnumerable<PXResult<Customer>> rows)
		{
			DateTime? date = Filter.Current.DocDate;

			List<ARDunningLetterList> returnList = new List<ARDunningLetterList>();
			foreach (PXResult<Customer, ARInvoiceWithDL, CustomerAlias, ARDunningCustomerClass, ARDunningCustomerClassAlias, ARBalances> row in rows)
			{
				Customer customer = row;
				CustomerAlias sharedCreditCustomer = row;
				ARInvoiceWithDL invoice = row;
				ARBalances balance = row;
				ARDunningCustomerClass nextDunningSetup = row;
				ARDunningCustomerClassAlias prevDunningSetup = row;
				int currentLevel = 0;
				if (invoice == null)
					continue;
				currentLevel = invoice.DunningLetterLevel ?? 0;

				int maxDunningLevel;
				if (MaxDunningLevels.TryGetValue(sharedCreditCustomer.CustomerClassID, out maxDunningLevel) && currentLevel ==  maxDunningLevel)
					continue;
				//Used for ProcessByCustomer mode
				if (nextDunningSetup.DueDays.HasValue
					&& invoice.DueDate.Value.AddDays(nextDunningSetup.DueDays.Value) >= date)
					continue;
				if (nextDunningSetup.DueDays.HasValue && prevDunningSetup.DueDays.HasValue
					&& currentLevel > 0 && invoice.DunningLetterDate.Value.AddDays(nextDunningSetup.DueDays.Value - prevDunningSetup.DueDays.Value) >= date)
					continue;

				var branch = PXAccess.GetBranch(invoice.BranchID);

				ARDunningLetterList item = new ARDunningLetterList();
				item.BAccountID = customer.SharedCreditCustomerID;
				item.OrganizationID = branch?.Organization?.OrganizationID;
				item.BranchID = branch?.BranchID;
				item.CustomerClassID = customer.CustomerClassID;
				item.DocBal = invoice.DocBal;
				item.DueDate = invoice.DueDate;
				item.DueDays = (nextDunningSetup.DaysToSettle ?? 0);
				item.DunningLetterLevel = currentLevel + 1;
				item.LastDunningLetterDate = invoice.DunningLetterDate;
				item.NumberOfOverdueDocuments = row.RowCount;
				item.CuryID = branch?.BaseCuryID;
				item.OrigDocAmt = PXAccess.FeatureInstalled<FeaturesSet.parentChildAccount>()
					? PXSelectJoinGroupBy<ARBalances,
						InnerJoin<Customer, On<Customer.bAccountID, Equal<ARBalances.customerID>>>,
						Where<ARBalances.branchID, Equal<Required<ARBalances.branchID>>,
							And<Customer.sharedCreditCustomerID, Equal<Required<Customer.sharedCreditCustomerID>>>>,
						Aggregate<GroupBy<ARBalances.customerID,
							Sum<ARBalances.currentBal>>>>.Select(this, item.BranchID, item.BAccountID).AsEnumerable()
						.Sum(cons => ((ARBalances)cons).CurrentBal)
					: balance.CurrentBal;

				BqlCommand cmd = new SelectFrom<Customer>
					.InnerJoin<ARRegister>.On<ARRegister.customerID.IsEqual<Customer.bAccountID>>
					.LeftJoin<Standalone.ARInvoice>.On<Standalone.ARInvoice.docType.IsEqual<ARRegister.docType>
						.And<Standalone.ARInvoice.refNbr.IsEqual<ARRegister.refNbr>>>
					.Where<Customer.sharedCreditCustomerID.IsEqual<@P.AsInt>
						.And<ARRegister.released.IsEqual<True>>
						.And<ARRegister.openDoc.IsEqual<True>>
						.And<ARRegister.voided.IsEqual<False>>
						.And<ARRegister.pendingPPD.IsNotEqual<True>>
						.And<ARRegister.docDate.IsLessEqual<@P.AsDateTime>>>();

				cmd = Filter.Current.AddOpenPaymentsAndCreditMemos ?? false
					? cmd.WhereAnd<Where<
						ARRegister.docType.IsEqual<ARDocType.invoice>
						.Or<ARRegister.docType.IsEqual<ARDocType.finCharge>>
						.Or<ARRegister.docType.IsEqual<ARDocType.debitMemo>>
						.Or<ARRegister.docType.IsEqual<ARDocType.payment>>
						.Or<ARRegister.docType.IsEqual<ARDocType.creditMemo>>
						.Or<ARRegister.docType.IsEqual<ARDocType.prepayment>>
						.Or<ARRegister.docType.IsEqual<ARDocType.refund>>>>()
					: cmd = cmd.WhereAnd<Where<
						ARRegister.docType.IsEqual<ARDocType.invoice>
						.Or<ARRegister.docType.IsEqual<ARDocType.finCharge>>
						.Or<ARRegister.docType.IsEqual<ARDocType.debitMemo>>>>();
				

				if (DunningLetterProcessTypeARSetup == DunningProcessType.ProcessByDocument)
				{
					cmd = cmd.WhereAnd<Where<Standalone.ARInvoice.revoked.IsEqual<False>.Or<Standalone.ARInvoice.revoked.IsNull>>>();
				}

				item.NumberOfDocuments = cmd.Select<Customer>(this, item.BAccountID, date).Count();

				returnList.Add(item);
			}

			return returnList;
		}

		#endregion

		#region Processing
		private static void DunningLetterProc(List<ARDunningLetterList> list, ARDunningLetterRecordsParameters filter)
		{
			ARDunningLetterProcess graph = PXGraph.CreateInstance<ARDunningLetterProcess>();

			var orgDLBranches = PXSelect<GL.DAC.Organization>.Select(graph).RowCast<GL.DAC.Organization>().ToDictionary(_ => _.OrganizationID, _ => _.DunningFeeBranchID);

			PXLongOperation.StartOperation(graph, delegate ()
			{
				bool errorsInProcessing = false;
				ARSetup arsetup = PXSelect<ARSetup>.Select(graph);
				bool autoRelease = arsetup.AutoReleaseDunningLetter == true;
				bool processByCutomer = arsetup.DunningLetterProcessType == DunningProcessType.ProcessByCustomer;

				List<ARDunningLetterList> uniqueList = arsetup.PrepareDunningLetters switch
				{
					ARSetup.prepareDunningLetters.ForEachBranch => DistinctBy(list, a => new { a.BAccountID, a.BranchID }).ToList(),
					ARSetup.prepareDunningLetters.ConsolidatedForCompany => DistinctBy(list, a => new { a.BAccountID, a.OrganizationID }).ToList(),
					ARSetup.prepareDunningLetters.ConsolidatedForAllCompanies => DistinctBy(list, a => a.BAccountID).ToList(),
					_ => throw new PXException()
				};

				foreach (ARDunningLetterList uniqueItem in uniqueList)
				{
					int? bAccountID = uniqueItem.BAccountID;

					int? branchID = null;

					if (arsetup.PrepareDunningLetters == ARSetup.prepareDunningLetters.ForEachBranch)
					{
						branchID = uniqueItem.BranchID;
					}
					else if (arsetup.PrepareDunningLetters == ARSetup.prepareDunningLetters.ConsolidatedForAllCompanies)
					{
						branchID = arsetup.DunningLetterBranchID;
					}
					else if(arsetup.PrepareDunningLetters == ARSetup.prepareDunningLetters.ConsolidatedForCompany)
					{
						var org = PXAccess.GetOrganizationByID(uniqueItem.OrganizationID);
						if (org.IsSingle)
						{
							var songleOrgBranch = PXAccess.GetBranchByBAccountID(org.BAccountID);
							branchID = songleOrgBranch.BranchID;
						}
						else if (orgDLBranches[uniqueItem.OrganizationID] == null)
						{
							throw new PXException(CS.Messages.DunningBranchIsNotSetForCompany, org.OrganizationCD);
						}
						else
						{
							var dlBranch = PXAccess.GetBranch(orgDLBranches[uniqueItem.OrganizationID]);
							branchID = dlBranch.BranchID;
						}
					}

					var branch = PXAccess.GetBranch(branchID);
					var organizationID = branch?.Organization?.OrganizationID;
					int? dueDays = uniqueItem.DueDays;

					List<int> dueDaysByLevel = new List<int>();
					foreach (ARDunningCustomerClass setup in SelectFrom<ARDunningCustomerClass>
								.Where<ARDunningCustomerClass.customerClassID.IsEqual<@P.AsString>>
								.OrderBy<ARDunningCustomerClass.dunningLetterLevel.Asc>.View.SelectMultiBound(graph, null, uniqueItem.CustomerClassID))
					{
						dueDaysByLevel.Add(setup.DueDays ?? 0);
					}

					List<int> levels = new List<int>();
					List<ARDunningLetterList> listToMerge = arsetup.PrepareDunningLetters switch
					{
						ARSetup.prepareDunningLetters.ForEachBranch => list.Where((item) => item.BAccountID == bAccountID && item.BranchID == branchID).ToList(),
						ARSetup.prepareDunningLetters.ConsolidatedForCompany => list.Where((item) => item.BAccountID == bAccountID && item.OrganizationID == organizationID).ToList(),
						ARSetup.prepareDunningLetters.ConsolidatedForAllCompanies => list.Where((item) => item.BAccountID == bAccountID).ToList(),
						_ => throw new PXException()
					};

					foreach (ARDunningLetterList item in listToMerge)
					{
						dueDays = dueDays < item.DueDays ? dueDays : item.DueDays;
						levels.Add(item.DunningLetterLevel ?? 0);
					}
					try
					{
						ARDunningLetter letterToRelease = graph.CreateDunningLetter(
							bAccountID,
							branchID,
							organizationID,
							dueDays, 
							levels,
							processByCutomer,
							arsetup.PrepareDunningLetters, 
							dueDaysByLevel,
							filter);
						try
						{
							if (autoRelease)
							{
								ARDunningLetterMaint.ReleaseProcess(letterToRelease);
							}
							foreach (ARDunningLetterList item in listToMerge)
							{
								PXProcessing.SetCurrentItem(item);
								PXProcessing.SetProcessed();
							}
						}
						catch (Exception exc)
						{

							string delimiter = "; ";
							string message = exc is PXOuterException outerExc
								? $"{outerExc.MessageNoPrefix}{delimiter}{string.Join(delimiter, outerExc.InnerMessages)}"
								: exc is PXException pxExc 
									? pxExc.MessageNoPrefix
									: exc.Message;

							foreach (ARDunningLetterList item in listToMerge)
							{
								PXProcessing.SetCurrentItem(item);
								PXProcessing.SetWarning(PXMessages.LocalizeFormat(Messages.DunningLetterNotReleased, message));
							}
						}
					}
					catch (Exception e)
					{
						foreach (ARDunningLetterList item in listToMerge)
						{
							PXProcessing.SetCurrentItem(item);
							PXProcessing.SetError(e);
							errorsInProcessing = true;
						}
					}
				}
				if (errorsInProcessing)
				{
					throw new PXException(Messages.DunningLetterNotCreated);
				}
			});
		}

		[Obsolete(PX.Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R1)]
		public static ARDunningLetter CreateDunningLetter(DunningLetterMassProcess graph, int? bAccountID, int? branchID, int? organizationID, DateTime? docDate, int? dueDays, List<int> includedLevels, bool includeNonOverdue, bool processByCutomer, string consolidationSettings, List<int> dueDaysByLevel, bool addOpenPaymentsAndCreditMemos = false)
		{
			ARDunningLetterProcess graph2 = PXGraph.CreateInstance<ARDunningLetterProcess>();
			var filter = new ARDunningLetterRecordsParameters
			{
				DocDate = docDate,
				IncludeNonOverdueDunning = includeNonOverdue,
				AddOpenPaymentsAndCreditMemos = addOpenPaymentsAndCreditMemos
			};
			filter = graph2.Filter.Insert(filter);

			var doc = graph2.CreateDunningLetter(bAccountID, branchID, organizationID, dueDays,
				includedLevels, processByCutomer, consolidationSettings, dueDaysByLevel, filter);

			return doc;
		}

		protected virtual ARDunningLetter CreateDunningLetter(int? bAccountID, int? branchID, int? organizationID,
			int? dueDays, List<int> includedLevels, bool processByCutomer, string consolidationSettings,
			List<int> dueDaysByLevel, ARDunningLetterRecordsParameters filter)
		{
			DateTime? docDate = filter.DocDate;
			bool includeNonOverdue = filter.IncludeNonOverdueDunning ?? false;
			bool addOpenPaymentsAndCreditMemos = filter.AddOpenPaymentsAndCreditMemos ?? false;

			this.Clear();

			int maxDunningLevel = dueDaysByLevel.Count;
			ARDunningLetter doc = CreateDunningLetterHeader(this, bAccountID, branchID, docDate, dueDays, consolidationSettings);
			doc = this.docs.Insert(doc);
			foreach (PXResult<ARInvoiceWithDL> result in GetInvoiceList(this, bAccountID, branchID, organizationID, docDate, includedLevels, includeNonOverdue, processByCutomer, consolidationSettings, dueDaysByLevel))
			{
				ARDunningLetterDetail docDet = CreateDunningLetterDetail(docDate, processByCutomer, result, dueDaysByLevel);
				doc.DunningLetterLevel = Math.Max(doc.DunningLetterLevel ?? 0, docDet.DunningLetterLevel ?? 0);
				this.docsDet.Insert(docDet);
			}

			doc.LastLevel = doc.DunningLetterLevel == maxDunningLevel;

			ARDunningCustomerClass dunningSetup = SelectFrom<ARDunningCustomerClass>
							.InnerJoin<Customer>
								.On<ARDunningCustomerClass.customerClassID.IsEqual<Customer.customerClassID>>
							.Where<Customer.bAccountID.IsEqual<@P.AsInt>
								.And<ARDunningCustomerClass.dunningLetterLevel.IsEqual<@P.AsInt>>>
							.View.SelectMultiBound(this, null, doc.BAccountID, doc.DunningLetterLevel);

			if (dunningSetup.DunningFee.HasValue && dunningSetup.DunningFee != 0m)
			{
				doc.DunningFee = dunningSetup.DunningFee;
			}

			if (addOpenPaymentsAndCreditMemos)
			{
				foreach (PXResult<ARPayment, Customer> payment in GetPaymentList(this, bAccountID, branchID, organizationID, docDate, processByCutomer, consolidationSettings))
				{
					ARDunningLetterDetail docDet = CreateDunningLetterDetailForPayment(payment);
					this.docsDet.Insert(docDet);
				}
			}

			doc = this.ExpandDunningLetter(doc, bAccountID, branchID, organizationID, dueDays, includedLevels,
										processByCutomer, consolidationSettings, dueDaysByLevel, filter);

			this.docs.Update(doc);
			this.Actions.PressSave();
			return doc;
		}

		protected virtual ARDunningLetter ExpandDunningLetter(ARDunningLetter doc, int? bAccountID, int? branchID,
			int? organizationID, int? dueDays, List<int> includedLevels, bool processByCutomer,
			string consolidationSettings, List<int> dueDaysByLevel, ARDunningLetterRecordsParameters filter) => doc;

		private static List<PXResult<ARInvoiceWithDL>> GetInvoiceList(PXGraph graph, int? bAccountID, int? branchID, int? organizationID, DateTime? docDate, List<int> includedLevels, bool includeNonOverdue, bool processByCutomer, string consolidationSettings, List<int> dueDaysByLevel)
		{
			if (processByCutomer)
			{
				var cmd = new PXSelectJoinGroupBy<ARInvoiceWithDL,
					InnerJoin<GL.Branch, On<GL.Branch.branchID, Equal<ARInvoiceWithDL.branchID>>>,
					Where<ARInvoiceWithDL.sharedCreditCustomerID, Equal<Required<ARInvoiceWithDL.customerID>>,
					   And<ARInvoiceWithDL.docDate, LessEqual<Required<ARInvoiceWithDL.docDate>>>>,
				   Aggregate<GroupBy<ARInvoiceWithDL.released,
					   GroupBy<ARInvoiceWithDL.refNbr,
					   GroupBy<ARInvoiceWithDL.docType>>>>>(graph);

				if (!includeNonOverdue)
				{
					cmd.WhereAnd<Where<ARInvoiceWithDL.dueDate, Less<Required<ARInvoice.dueDate>>>>();
				}
				else
				{
					cmd.WhereAnd<Where<Required<ARInvoice.dueDate>, IsNotNull>>();
				}

				int? sourceID = null;
				switch (consolidationSettings)
				{
					case ARSetup.prepareDunningLetters.ForEachBranch:
						cmd.WhereAnd<Where<ARInvoiceWithDL.branchID, Equal<Required<ARInvoiceWithDL.branchID>>>>();
						sourceID = branchID;
						break;

					case ARSetup.prepareDunningLetters.ConsolidatedForCompany:
						cmd.WhereAnd<Where<GL.Branch.organizationID, Equal<Required<GL.Branch.organizationID>>>>();
						sourceID = organizationID;
						break;
				}

				return cmd.Select(bAccountID, docDate, docDate, sourceID).ToList();
			}
			else
			{
				List<PXResult<ARInvoiceWithDL>> results = new List<PXResult<ARInvoiceWithDL>>();

				foreach (int level in includedLevels)
				{
					var cmd = new PXSelectJoinGroupBy<ARInvoiceWithDL,
						InnerJoin<GL.Branch, On<GL.Branch.branchID, Equal<ARInvoiceWithDL.branchID>>>,
						Where<ARInvoiceWithDL.sharedCreditCustomerID, Equal<Required<ARInvoiceWithDL.customerID>>,
							And<ARInvoiceWithDL.revoked, Equal<False>,
							And<ARInvoiceWithDL.dueDate, Less<Required<ARInvoice.dueDate>>,
							And<ARInvoiceWithDL.docDate, LessEqual<Required<ARInvoiceWithDL.docDate>>,
							And<Where<ARInvoiceWithDL.dunningLetterLevel, Equal<Required<ARDunningLetter.dunningLetterLevel>>,
								Or<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull,
									And<Required<ARDunningLetter.dunningLetterLevel>, Equal<int0>>>>>>>>>>,
						Aggregate<GroupBy<ARInvoiceWithDL.dunningLetterLevel,
							 GroupBy<ARInvoiceWithDL.released,
							 GroupBy<ARInvoiceWithDL.refNbr,
							 GroupBy<ARInvoiceWithDL.docType>>>>>>(graph);

					int? sourceID = null;
					switch (consolidationSettings)
					{
						case ARSetup.prepareDunningLetters.ForEachBranch:
							cmd.WhereAnd<Where<ARInvoiceWithDL.branchID, Equal<Required<ARInvoiceWithDL.branchID>>>>();
							sourceID = branchID;
							break;

						case ARSetup.prepareDunningLetters.ConsolidatedForCompany:
							cmd.WhereAnd<Where<GL.Branch.organizationID, Equal<Required<GL.Branch.organizationID>>>>();
							sourceID = organizationID;
							break;
					}

					results = results.Concat(cmd.Select(bAccountID, docDate.Value.AddDays(-1 * dueDaysByLevel[level - 1]), docDate, level - 1, level - 1, sourceID)).ToList();
					if (level == 1 && includeNonOverdue)
					{
						var cmdLvl1 = new PXSelectJoinGroupBy<ARInvoiceWithDL,
							InnerJoin<GL.Branch, On<GL.Branch.branchID, Equal<ARInvoiceWithDL.branchID>>>,
					   Where<ARInvoiceWithDL.sharedCreditCustomerID, Equal<Required<ARInvoiceWithDL.customerID>>,
						   And<ARInvoiceWithDL.revoked, Equal<False>,
						   And2<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull,
							Or<ARInvoiceWithDL.dunningLetterLevel, Equal<int0>>>,
						   And<ARInvoiceWithDL.dueDate, Less<Required<ARInvoiceWithDL.docDate>>,
						   And<ARInvoiceWithDL.dueDate, GreaterEqual<Required<ARInvoiceWithDL.docDate>>,
						   And<ARInvoiceWithDL.docDate, LessEqual<Required<ARInvoiceWithDL.docDate>>>>>>>>,
					   Aggregate<GroupBy<ARInvoiceWithDL.dunningLetterLevel,
							GroupBy<ARInvoiceWithDL.released,
							GroupBy<ARInvoiceWithDL.refNbr,
							GroupBy<ARInvoiceWithDL.docType>>>>>>(graph);

						switch (consolidationSettings)
						{
							case ARSetup.prepareDunningLetters.ForEachBranch:
								cmdLvl1.WhereAnd<Where<ARInvoiceWithDL.branchID, Equal<Required<ARInvoiceWithDL.branchID>>>>();
								break;

							case ARSetup.prepareDunningLetters.ConsolidatedForCompany:
								cmdLvl1.WhereAnd<Where<GL.Branch.organizationID, Equal<Required<GL.Branch.organizationID>>>>();
								break;
						}

						results = results.Concat(cmdLvl1.Select(bAccountID, docDate, docDate.Value.AddDays(-1 * dueDaysByLevel[0]), docDate, sourceID)).ToList();
					}
				}
				if (includeNonOverdue)
				{
					var cmdNonOverdue = new PXSelectJoinGroupBy<ARInvoiceWithDL,
						InnerJoin<GL.Branch, On<GL.Branch.branchID, Equal<ARInvoiceWithDL.branchID>>>,
						Where<ARInvoiceWithDL.sharedCreditCustomerID, Equal<Required<ARInvoiceWithDL.customerID>>,
							And<ARInvoiceWithDL.revoked, Equal<False>,
							And2<Where<ARInvoiceWithDL.dunningLetterLevel, IsNull,
								Or<ARInvoiceWithDL.dunningLetterLevel, Equal<int0>>>,
							And<ARInvoiceWithDL.dueDate, GreaterEqual<Required<ARInvoiceWithDL.docDate>>,
							And<ARInvoiceWithDL.docDate, LessEqual<Required<ARInvoiceWithDL.docDate>>>>>>>,
						Aggregate<GroupBy<ARInvoiceWithDL.dunningLetterLevel,
							 GroupBy<ARInvoiceWithDL.released,
							 GroupBy<ARInvoiceWithDL.refNbr,
							 GroupBy<ARInvoiceWithDL.docType>>>>>>(graph);

					int? sourceID = null;
					switch (consolidationSettings)
					{
						case ARSetup.prepareDunningLetters.ForEachBranch:
							cmdNonOverdue.WhereAnd<Where<ARInvoiceWithDL.branchID, Equal<Required<ARInvoiceWithDL.branchID>>>>();
							sourceID = branchID;
							break;

						case ARSetup.prepareDunningLetters.ConsolidatedForCompany:
							cmdNonOverdue.WhereAnd<Where<GL.Branch.organizationID, Equal<Required<GL.Branch.organizationID>>>>();
							sourceID = organizationID;
							break;
					}

					results = results.Concat(cmdNonOverdue.Select(bAccountID, docDate, docDate, sourceID)).ToList();
				}
				return results;
			}
		}

		private static List<PXResult<ARPayment, Customer>> GetPaymentList(PXGraph graph, int? bAccountID, int? branchID, int? organizationID, DateTime? docDate, bool processByCutomer, string consolidationSettings)
		{
			BqlCommand cmd = new SelectFrom<ARPayment>
				.InnerJoin<Customer>.On<Customer.bAccountID.IsEqual<ARPayment.customerID>>
				.InnerJoin<GL.Branch>.On<GL.Branch.branchID.IsEqual<ARPayment.branchID>>
				.LeftJoin<Standalone.ARInvoice>.On<Standalone.ARInvoice.docType.IsEqual<ARPayment.docType>
						.And<Standalone.ARInvoice.refNbr.IsEqual<ARPayment.refNbr>>>
				.Where<ARPayment.released.IsEqual<True>
					.And<ARPayment.openDoc.IsEqual<True>>
					.And<ARPayment.voided.IsEqual<False>>
					.And<ARPayment.pendingPPD.IsNotEqual<True>>
					.And<ARPayment.docType.IsEqual<ARDocType.payment>
						.Or<ARPayment.docType.IsEqual<ARDocType.creditMemo>>
						.Or<ARPayment.docType.IsEqual<ARDocType.prepayment>>
						.Or<ARPayment.docType.IsEqual<ARDocType.refund>>>
					.And<Customer.sharedCreditCustomerID.IsEqual<@P.AsInt>>
					.And<ARPayment.docDate.IsLessEqual<@P.AsDateTime>>>();

			int? sourceID = null;

			switch (consolidationSettings)
			{
				case ARSetup.prepareDunningLetters.ForEachBranch:
					cmd = cmd.WhereAnd<Where<ARPayment.branchID.IsEqual<@P.AsInt>>>();
					sourceID = branchID;
					break;

				case ARSetup.prepareDunningLetters.ConsolidatedForCompany:
					cmd = cmd.WhereAnd<Where<GL.Branch.organizationID.IsEqual<@P.AsInt>>>();
					sourceID = organizationID;
					break;
			}

			if (!processByCutomer)
			{
				cmd = cmd.WhereAnd<Where<Standalone.ARInvoice.revoked.IsEqual<False>.Or<Standalone.ARInvoice.revoked.IsNull>>>();
			}
			return cmd.CreateView(graph).SelectMulti(bAccountID, docDate, sourceID).Cast<PXResult<ARPayment, Customer>>().ToList();
		}

		private static ARDunningLetter CreateDunningLetterHeader(PXGraph graph, int? bAccountID, int? branchID, DateTime? docDate, int? dueDays, string consolidationSettings)
		{
			ARDunningLetter doc = new ARDunningLetter();
			doc.BAccountID = bAccountID;
			doc.BranchID = branchID;
			doc.DunningLetterDate = docDate;
			doc.Deadline = docDate.Value.AddDays(dueDays.Value);
			doc.ConsolidationSettings = consolidationSettings;
			doc.Released = false;
			doc.Printed = false;
			doc.Emailed = false;
			doc.LastLevel = false;
			Customer customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(graph, bAccountID);
			doc.DontPrint = customer.PrintDunningLetters == false;
			doc.DontEmail = customer.MailDunningLetters == false;
			return doc;
		}

		private static ARDunningLetterDetail CreateDunningLetterDetail(DateTime? docDate, bool processByCutomer, ARInvoiceWithDL invoice, List<int> dueDaysByLevel)
		{
			ARDunningLetterDetail detail = new ARDunningLetterDetail();

			detail.CuryOrigDocAmt = invoice.CuryOrigDocAmt;
			detail.CuryDocBal = invoice.CuryDocBal;
			detail.CuryID = invoice.CuryID;
			detail.OrigDocAmt = invoice.OrigDocAmt;
			detail.DocBal = invoice.DocBal;
			detail.DueDate = invoice.DueDate;
			detail.DocType = invoice.DocType;
			detail.RefNbr = invoice.RefNbr;
			detail.BAccountID = invoice.CustomerID;
			detail.DunningLetterBAccountID = invoice.SharedCreditCustomerID;
			detail.DocDate = invoice.DocDate;
			detail.Overdue = invoice.DueDate < docDate;
			if ((processByCutomer && invoice.DueDate >= docDate) || invoice.DueDate.Value.AddDays(dueDaysByLevel[invoice.DunningLetterLevel ?? 0]) >= docDate)
			{
				detail.DunningLetterLevel = 0;
			}
			else
			{
				detail.DunningLetterLevel = (invoice.DunningLetterLevel ?? 0) + 1;
			}
			return detail;
		}

		private static ARDunningLetterDetail CreateDunningLetterDetailForPayment(PXResult<ARPayment, Customer> paymentDoc)
		{
			ARDunningLetterDetail detail = new ARDunningLetterDetail();

			ARPayment payment = paymentDoc;
			Customer customer = paymentDoc;

			detail.CuryOrigDocAmt = payment.CuryOrigDocAmt;
			detail.CuryDocBal = payment.CuryDocBal;
			detail.CuryID = payment.CuryID;
			detail.OrigDocAmt = payment.OrigDocAmt;
			detail.DocBal = payment.DocBal;
			detail.DocType = payment.DocType;
			detail.RefNbr = payment.RefNbr;
			detail.BAccountID = payment.CustomerID;
			detail.DunningLetterBAccountID = customer.SharedCreditCustomerID;
			detail.DocDate = payment.DocDate;
			detail.Overdue = false;
			detail.DunningLetterLevel = 0;
			return detail;
		}
		#endregion
	}

	[Obsolete(PX.Objects.Common.Messages.ItemIsObsoleteAndWillBeRemoved2024R1)]
	[PXHidden]
	public class DunningLetterMassProcess : PXGraph<DunningLetterMassProcess>
	{
		[PXViewName("DunningLetter")]
		public PXSelect<ARDunningLetter> docs;
		[PXViewName("DunningLetterDetail")]
		public PXSelect<ARDunningLetterDetail, 
			Where<ARDunningLetterDetail.dunningLetterID, Equal<Required<ARDunningLetter.dunningLetterID>>>> docsDet;
	}
}
