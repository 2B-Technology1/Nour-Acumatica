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
using System.Net.Http;
using System.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.AP;
using PX.OAuthClient.DAC;
using PX.Objects.Localizations.GB.HMRC.DAC;
using PX.Objects.Localizations.GB.HMRC.Model;
using PX.Objects.GL.Attributes;
using System.Threading.Tasks;

namespace PX.Objects.Localizations.GB.HMRC
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class VATMaint : PXGraph<VATMaint>
	{
		public PXFilter<VATPeriodFilter> Period_Header;
		public PXCancel<VATPeriodFilter> Cancel;

		[InjectDependency]
		public IOptions<MtdOptions> Options { get; set; }

		[InjectDependency]
		public IHttpClientFactory HttpClientFactory { get; set; }

		public bool IsTestEnvironment => Options?.Value?.IsTestEnvironment ?? false;

		private string ObligationsTestScenario => Options?.Value?.ObligationsTestHeader; // https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0#_retrieve-vat-obligations_get_accordion

		private VATApi VatProvider => VATApi.GetVatApi(this);

		#region VATRows
		public PXSelect<VATRow> VATRows;

		protected virtual IEnumerable vATRows()
		{
			return this.Caches<VATRow>().Inserted;
		}
		#endregion

		#region Obligations
		public PXSelect<DAC.Obligation> Obligations;
		public virtual IEnumerable obligations()
		{
			PXCache<DAC.Obligation> cache = this.Caches<DAC.Obligation>();

			IEnumerable<DAC.Obligation> obligations = (IEnumerable<DAC.Obligation>)this.Caches<DAC.Obligation>().Inserted;
			if (obligations.Any())
				return obligations;
			else
			{
				if (Period_Header.Current?.EndDate != null)
				{
					ObligationsRequest req = new ObligationsRequest() { from = Period_Header.Current.StartDate.Value, to = Period_Header.Current.EndDate.Value, status = null };
					ObligationResponse obligationResponse = null;

					string testScenario = string.Empty;
					if (this.IsTestEnvironment && !string.IsNullOrEmpty(this.ObligationsTestScenario))
					{
						testScenario = this.ObligationsTestScenario;
					}

					VATApi provider = VatProvider;
					if (provider == null)
					{
						PXTrace.WriteError(Messages.CouldNotInitApi);
					}

					Guid key = Guid.NewGuid();
					// Acuminator disable once PX1080 LongRunOperationInDataViewDelegate [The result of an api call is explicitely awaited]
					PXLongOperation.StartOperation(key, () =>
					{
						try
						{
							var result = this.VatProvider.Obligations(req, this.headersReturn.Current, testScenario).GetAwaiter().GetResult();
							PXLongOperation.SetCustomInfo(result, $"Result");
						}
						catch (Exception e)
						{
							PXLongOperation.SetCustomInfo(e, $"Error");
						}
					}
					);
					PXLongOperation.WaitCompletion(key);

					object error = PXLongOperation.GetCustomInfo(key, $"Error");
					if (error is Exception)
					{
						PXTrace.WriteError((Exception)error);
					}
					else
					{
						object customInfo = PXLongOperation.GetCustomInfo(key, $"Result");
						if (customInfo != null)
							obligationResponse = (ObligationResponse)customInfo;
					}

					this.Obligations.Cache.Clear();
					if (obligationResponse != null)
						foreach (var o in obligationResponse.obligations)
						{
							this.Obligations.Insert(new DAC.Obligation()
							{
								Start = o.start,
								End = o.end,
								Due = o.due,
								Status = o.status,
								PeriodKey = o.periodKey,
								Received = o.received
							});
						}
				}
			}

			return (IEnumerable<DAC.Obligation>)cache.Inserted;
		}
		#endregion

		#region BQL Selectors
		public PXSelect<
			TaxPeriod,
			Where<TaxPeriod.organizationID, Equal<Current<VATPeriodFilter.organizationID>>,
				And<TaxPeriod.vendorID, Equal<Current<VATPeriodFilter.vendorID>>,
				And<TaxPeriod.taxPeriodID, Equal<Current<VATPeriodFilter.taxPeriodID>>>>>>
			Period;

		public PXSelect<
			Vendor,
			Where<Vendor.bAccountID, Equal<Current<VATPeriodFilter.vendorID>>>>
			Vendor;
		#endregion

		#region internal
		public override bool IsDirty { get { return false; } }
		private bool HeadersCalculated => headersReturn.Current != null;
		#endregion

		#region Period_Details
		public PXSelectJoin<
							TaxReportLine,
							LeftJoin<TaxHistoryReleased,
								On<TaxHistoryReleased.vendorID, Equal<TaxReportLine.vendorID>,
								And<TaxHistoryReleased.lineNbr, Equal<TaxReportLine.lineNbr>,
								And<TaxHistory.taxReportRevisionID, Equal<TaxReportLine.taxReportRevisionID>>>>>,
							Where<False, Equal<True>>,
							OrderBy<
								Asc<TaxReportLine.sortOrder,
								Asc<TaxReportLine.taxZoneID>>>>
							Period_Details;

		public PXSelectJoinGroupBy<
			TaxReportLine,
			LeftJoin<TaxHistoryReleased,
				On<TaxHistoryReleased.vendorID, Equal<TaxReportLine.vendorID>,
				And<TaxHistoryReleased.lineNbr, Equal<TaxReportLine.lineNbr>,
				And<TaxHistoryReleased.taxPeriodID, Equal<Current<VATPeriodFilter.taxPeriodID>>,
				And<TaxHistoryReleased.revisionID, LessEqual<Current<VATPeriodFilter.revisionId>>>>>>>,
			Where<TaxReportLine.vendorID, Equal<Current<VATPeriodFilter.vendorID>>,
				And<TaxReportLine.tempLine, Equal<False>,
				And2<
					Where<TaxReportLine.tempLineNbr, IsNull,
						Or<TaxHistoryReleased.vendorID, IsNotNull>>,
					And<Where<TaxReportLine.hideReportLine, IsNull,
						Or<TaxReportLine.hideReportLine, Equal<False>>>>>
				>>,
			Aggregate<
				GroupBy<TaxReportLine.lineNbr,
					Sum<TaxHistoryReleased.filedAmt,
					Sum<TaxHistoryReleased.reportFiledAmt>>>>>
			Period_Details_Expanded;

		protected virtual IEnumerable period_Details()
		{
			VATPeriodFilter filter = Period_Header.Current;

			using (new PXReadBranchRestrictedScope(filter.OrganizationID.SingleToArray(), filter.BranchID.SingleToArrayOrNull()))
			{
				PXResultset<TaxReportLine> trl = Period_Details_Expanded.Select();
				foreach (PXResult<TaxReportLine, TaxHistoryReleased> line in trl)
				{
					TaxHistoryReleased th1 = (TaxHistoryReleased)line;
					if (th1 == null)
						continue;
					if (th1.ReportFiledAmt == null)
						th1.ReportFiledAmt = 0;

					TaxReportLine reportLine = line;
					switch (reportLine.ReportLineNbr)
					{
						//Box 5 should always contain a positive value
						case "5": th1.ReportFiledAmt = Math.Abs(th1.ReportFiledAmt.Value); break;
						//Boxes 7-9 contain pound (no pence) values
						case "6":
						case "7":
						case "8":
						case "9": th1.ReportFiledAmt = Math.Floor(th1.ReportFiledAmt.Value); break;
					}
				}
				return trl;
			}
		}

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public VATMaint()
		{
			sendVATreturn.SetEnabled(false);
			checkVATReturn.SetEnabled(false);
			PXUIFieldAttribute.SetVisibility<TaxReportLine.lineNbr>(Period_Details.Cache, null, PXUIVisibility.Invisible);
			PXUIFieldAttribute.SetVisibility<TaxReportLine.lineNbr>(Period_Details.Cache, null, PXUIVisibility.Invisible);
		}

		#region VATPeriodFilter Handlers

		[PXDefault]
		[PXUIField(DisplayName = "Tax Period", Visibility = PXUIVisibility.Visible)]
		[FinPeriodID]
		[PXSelector(
			 typeof(Search<
				 TaxPeriod.taxPeriodID,
				 Where<TaxPeriod.vendorID, Equal<Current<TaxPeriodFilter.vendorID>>,
					 And<TaxPeriod.organizationID, Equal<Current<TaxPeriodFilter.organizationID>>,
					 And<TaxPeriod.status, Equal<TaxPeriodStatus.closed>>>>,
				 OrderBy<
					 Desc<TaxPeriod.taxPeriodID>>>),
			 typeof(TaxPeriod.taxPeriodID), typeof(TaxPeriod.startDateUI), typeof(TaxPeriod.endDateUI), typeof(TaxPeriod.status),
			 SelectorMode = PXSelectorMode.NoAutocomplete,
			 DirtyRead = true)]
		protected void VATPeriodFilter_TaxPeriodID_CacheAttached(PXCache sender) { }

		protected void baseVATPeriodFilterRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			VATPeriodFilter filter = e.Row as VATPeriodFilter;

			if (filter?.OrganizationID == null)
				return;

			Organization organization = OrganizationMaint.FindOrganizationByID(this, filter.OrganizationID);

			if (organization.FileTaxesByBranches == true && filter.BranchID == null || filter.VendorID == null || filter.TaxPeriodID == null)
				return;

			int? maxRevision = ReportTaxProcess.CurrentRevisionId(sender.Graph, filter.OrganizationID, filter.BranchID, filter.VendorID, filter.TaxPeriodID);
			TaxPeriod taxPeriod = Period.Select();
			SetFilter(filter);

			PXUIFieldAttribute.SetEnabled<VATPeriodFilter.revisionId>(sender, null, maxRevision > 1);
			PXUIFieldAttribute.SetEnabled<VATPeriodFilter.taxPeriodID>(sender, null, true);
		}

		private void SetFilter(VATPeriodFilter filter)
		{
			TaxPeriod taxPeriod = Period.Select();
			filter.StartDate = taxPeriod?.StartDateUI;
			filter.EndDate = taxPeriod?.EndDate != null ? (DateTime?)(((DateTime)taxPeriod.EndDate).AddDays(-1)) : null;
		}

		protected void VATPeriodFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			baseVATPeriodFilterRowSelected(sender, e);
			bool revisionSelected = (((VATPeriodFilter)(e.Row))?.RevisionId != null);
			this.sendVATreturn.SetEnabled(revisionSelected && HeadersCalculated);
			this.checkVATReturn.SetEnabled(revisionSelected && HeadersCalculated);
			testFraudHeaders.SetEnabled(HeadersCalculated);
			testFraudHeaders.SetVisible(IsTestEnvironment);
		}

		protected virtual void baseTaxPeriodFilterRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			VATPeriodFilter filter = (VATPeriodFilter)e.Row;
			if (filter == null) return;

			if (!sender.ObjectsEqual<VATPeriodFilter.organizationID, VATPeriodFilter.branchID>(e.Row, e.OldRow))
			{
				List<PXView> views = this.Views.Select(view => view.Value).ToList();
				foreach (var view in views) view.Clear();
			}

			if (!sender.ObjectsEqual<VATPeriodFilter.organizationID>(e.Row, e.OldRow)
				|| !sender.ObjectsEqual<VATPeriodFilter.branchID>(e.Row, e.OldRow)
				|| !sender.ObjectsEqual<VATPeriodFilter.vendorID>(e.Row, e.OldRow))
			{
				if (filter.OrganizationID != null && filter.VendorID != null)
				{
					PX.Objects.TX.TaxPeriod taxper = TaxYearMaint.FindPreparedPeriod(this, filter.OrganizationID, filter.VendorID);

					if (taxper != null)
					{
						filter.TaxPeriodID = taxper.TaxPeriodID;
					}
					else
					{
						taxper = TaxYearMaint.FindLastClosedPeriod(this, filter.OrganizationID, filter.VendorID);
						filter.TaxPeriodID = taxper != null ? taxper.TaxPeriodID : null;
					}
				}
				else
				{
					filter.TaxPeriodID = null;
				}
			}

			Organization organization = OrganizationMaint.FindOrganizationByID(this, filter.OrganizationID);

			if (!sender.ObjectsEqual<VATPeriodFilter.organizationID>(e.Row, e.OldRow)
				|| !sender.ObjectsEqual<VATPeriodFilter.branchID>(e.Row, e.OldRow)
				|| !sender.ObjectsEqual<VATPeriodFilter.vendorID>(e.Row, e.OldRow)
				|| !sender.ObjectsEqual<VATPeriodFilter.taxPeriodID>(e.Row, e.OldRow)
				|| filter.RevisionId == null)
			{
				if (filter.OrganizationID != null
					&& (filter.BranchID != null && organization.FileTaxesByBranches == true || organization.FileTaxesByBranches != true)
					&& filter.VendorID != null && filter.TaxPeriodID != null)
				{
					filter.RevisionId = ReportTaxProcess.CurrentRevisionId(this, filter.OrganizationID, filter.BranchID, filter.VendorID, filter.TaxPeriodID);
				}
				else
				{
					filter.RevisionId = null;
				}
			}
		}

		protected void VATPeriodFilter_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			VATPeriodFilter filter = (VATPeriodFilter)e.Row;
			if (filter.OrganizationID != null && filter.VendorID != null && filter.TaxPeriodID != null)
			{
				SelectPeriodKey(filter);
			}
		}

		protected void VATPeriodFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			baseTaxPeriodFilterRowUpdated(sender, e);
			VATPeriodFilter filter = (VATPeriodFilter)e.Row;

			if (!sender.ObjectsEqual<VATPeriodFilter.organizationID>(e.Row, e.OldRow)
				|| !sender.ObjectsEqual<VATPeriodFilter.branchID>(e.Row, e.OldRow)
				|| !sender.ObjectsEqual<VATPeriodFilter.vendorID>(e.Row, e.OldRow)
				|| !sender.ObjectsEqual<VATPeriodFilter.taxPeriodID>(e.Row, e.OldRow)
			 )
			{
				if (!sender.ObjectsEqual<VATPeriodFilter.organizationID>(e.Row, e.OldRow)
					|| !sender.ObjectsEqual<VATPeriodFilter.branchID>(e.Row, e.OldRow))
				{
					if (!sender.ObjectsEqual<VATPeriodFilter.organizationID>(e.Row, e.OldRow))
					{
						Period_Header.SetValueExt<VATPeriodFilter.branchID>(filter, null);
					}
					this.Obligations.Cache.Clear();
					Period_Header.SetValueExt<VATPeriodFilter.vendorID>(filter, null);
					Period_Header.SetValueExt<VATPeriodFilter.taxPeriodID>(filter, null);
					Period_Header.SetValueExt<VATPeriodFilter.revisionId>(filter, null);
					Period_Header.SetValueExt<VATPeriodFilter.startDate>(filter, null);
					Period_Header.SetValueExt<VATPeriodFilter.endDate>(filter, null);
				}
				Period_Header.SetValueExt<VATPeriodFilter.periodKey>(filter, null);
				Period_Header.SetValueExt<VATPeriodFilter.start>(filter, null);
				Period_Header.SetValueExt<VATPeriodFilter.end>(filter, null);
				Period_Header.SetValueExt<VATPeriodFilter.due>(filter, null);
				Period_Header.SetValueExt<VATPeriodFilter.status>(filter, null);
				Period_Header.SetValueExt<VATPeriodFilter.received>(filter, null);
				VATRows.Cache.Clear();
			}

			if (!(sender.ObjectsEqual<VATPeriodFilter.taxPeriodID>(e.Row, e.OldRow)
				|| filter.OrganizationID == null
				||
					((OrganizationMaint.FindOrganizationByID(this, filter.OrganizationID)?.FileTaxesByBranches ?? false)
					 && filter.BranchID == null)))
			{
				SelectPeriodKey(filter);
			}
		}

		private void SelectPeriodKey(VATPeriodFilter filter)
		{
			if (!HeadersCalculated) return;
			try
			{
				DAC.Obligation o;
				TaxPeriod taxPeriod = Period.Select();
				filter.StartDate = taxPeriod?.StartDateUI;
				filter.EndDate = taxPeriod?.EndDate != null ? (DateTime?)(((DateTime)taxPeriod.EndDate).AddDays(-1)) : null;

				if (filter.StartDate != null && filter.EndDate != null)
				{
					var obligations = Obligations.Select();

					foreach (DAC.Obligation ob in obligations)
					{
						if (ob.End == filter.EndDate.Value)
						{
							Obligations.Current = ob;
							break;
						}
					}

					o = Obligations.Current;
					if (o != null)
					{
						filter.PeriodKey = o.PeriodKey;
						filter.Start = o.Start;
						filter.End = o.End;
						filter.Status = o.Status;
						filter.Due = o.Due;
						filter.Received = o.Received;
					}
				}
			}
			catch (Exception ex)
			{
				PXTrace.WriteError(ex.ToString());
			}
		}

		public void _(Events.RowUpdating<VATPeriodFilter> e)
		{
			VATPeriodFilter newVATPeriodFilter = e.NewRow;
			// If update is emptying old value, or leaving this value blank, do not check
			if (newVATPeriodFilter.OrganizationID == null) return;
			// parentOrganization refers top organization parent of potential branch
			Organization parentOrganization = SelectFrom<Organization>.Where<Organization.organizationID.IsEqual<VATPeriodFilter.organizationID.AsOptional>>.View.Select(this, newVATPeriodFilter.OrganizationID);

			// Split logic between checking parentOrganization if it files taxes by branch, or branch organization otherwise.
			if (!parentOrganization?.FileTaxesByBranches ?? false) // IF it DOESN'T File taxes by branches
			{
				BAccountR bAccountR = SelectFrom<BAccountR>.Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.AsOptional>>.View.Select(this, parentOrganization.BAccountID);
				BAccountMTDApplication bAccountApplication = SelectFrom<BAccountMTDApplication>.Where<BAccountMTDApplication.bAccountID.IsEqual<Organization.bAccountID.AsOptional>>.View.Select(this, parentOrganization.BAccountID);

				if (bAccountApplication == null || bAccountApplication?.ApplicationID == null)
				{
					e.Cache.RaiseExceptionHandling<VATPeriodFilter.organizationID>(
						newVATPeriodFilter,
						parentOrganization.OrganizationCD,
						new PXSetPropertyException<VATPeriodFilter.organizationID>(Messages.CompanyFieldEmpty, PXErrorLevel.Error,
						new object[]
						{
							parentOrganization.OrganizationName,
							PXUIFieldAttribute.GetDisplayName<BAccountMTDApplication.applicationID>(Caches[typeof(BAccountMTDApplication)])
						}));
				}
				if (bAccountR?.TaxRegistrationID == null)
				{
					e.Cache.RaiseExceptionHandling<VATPeriodFilter.organizationID>(
						newVATPeriodFilter,
						parentOrganization.OrganizationCD,
						new PXSetPropertyException<VATPeriodFilter.organizationID>(Messages.CompanyFieldEmpty, PXErrorLevel.Error,
						new object[]
						{
							parentOrganization.OrganizationName,
							PXUIFieldAttribute.GetDisplayName<BAccountR.taxRegistrationID>(Caches[typeof(BAccountR)])
						}
						));
				}
			}
			else // IF it does
			{
				if (newVATPeriodFilter.BranchID == null) return;

				Branch branch = SelectFrom<Branch>.Where<Branch.branchID.IsEqual<VATPeriodFilter.branchID.AsOptional>>.View.Select(this, newVATPeriodFilter.BranchID);
				BAccountR bAccountR = SelectFrom<BAccountR>.Where<BAccountR.bAccountID.IsEqual<Organization.bAccountID.AsOptional>>.View.Select(this, branch?.BAccountID);
				BAccountMTDApplication bAccountApplication = SelectFrom<BAccountMTDApplication>.Where<BAccountMTDApplication.bAccountID.IsEqual<Branch.bAccountID.AsOptional>>.View.Select(this, branch.BAccountID);

				if (bAccountApplication == null || bAccountApplication?.ApplicationID == null)
				{
					e.Cache.RaiseExceptionHandling<VATPeriodFilter.branchID>(
						newVATPeriodFilter,
						bAccountR.AcctCD,
						new PXSetPropertyException<VATPeriodFilter.branchID>(Messages.BranchFieldEmpty, PXErrorLevel.Error,
						new object[]
						{
							branch.AcctName,
							PXUIFieldAttribute.GetDisplayName<BAccountMTDApplication.applicationID>(Caches[typeof(BAccountMTDApplication)])
						}));
				}
				if (bAccountR?.TaxRegistrationID == null)
				{
					e.Cache.RaiseExceptionHandling<VATPeriodFilter.branchID>(
						newVATPeriodFilter,
						bAccountR.AcctCD,
						new PXSetPropertyException<VATPeriodFilter.branchID>(Messages.BranchFieldEmpty, PXErrorLevel.Error,
						new object[]
						{
							branch.AcctName,
							PXUIFieldAttribute.GetDisplayName<BAccountR.taxRegistrationID>(Caches[typeof(BAccountR)])
						}));
				}
			}
		}

		public void _(Events.FieldDefaulting<VATPeriodFilter.organizationID> e)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.branch>()) return;
			//Triggering defaulting of the company manually for single company scenario
			OrganizationAttribute orgAttr = new OrganizationAttribute(true);
			PXDefaultAttribute defaultAttr = orgAttr.GetAttribute<PXDefaultAttribute>();
			defaultAttr.FieldDefaulting(e.Cache, e.Args);
		}

		#endregion

		#region Actions

		#region Calculate
		public PXAction<VATPeriodFilter> calculate;
		[PXProcessButton]
		[PXUIField(DisplayName = "Calculate", Visible = false)]
		public virtual IEnumerable Calculate(PXAdapter adapter)
		{
			string payload = adapter.CommandArguments;

			if (string.IsNullOrEmpty(payload))
				return adapter.Get();

			UpdateGovHeadersData(this, payload);

			if (!string.IsNullOrEmpty(Period_Header.Current?.TaxPeriodID))
			{
				SelectPeriodKey(Period_Header.Current);
			}

			return adapter.Get();
		}
		#endregion

		#region TestFraudHeaders
		public PXAction<VATPeriodFilter> testFraudHeaders;
		[PXUIField(DisplayName = Messages.TestFraudHeaders, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable TestFraudHeaders(PXAdapter adapter)
		{
			VATApi provider = this.VatProvider;
			if (provider == null)
			{
				throw new PXException(Messages.CouldNotInitApi);
			}

			Guid key = Guid.NewGuid();
			PXLongOperation.StartOperation(key, () =>
			{
				try
				{
					provider.TestFraudPrevention(headersReturn.Current).GetAwaiter().GetResult();
				}
				catch (Exception e)
				{
					PXLongOperation.SetCustomInfo(e, $"Error");
				}
			}
			);
			PXLongOperation.WaitCompletion(key);

			object error = PXLongOperation.GetCustomInfo(key, $"Error");
			if (error is Exception)
			{
				Exception e = (Exception)error;
				PXTrace.WriteError(e);
				throw new PXException(e.Message);
			}
			else
			{
				throw new PXException(Messages.HeadersValidated);
			}

			return adapter.Get();
		}
		#endregion

		#region checkVATReturn
		public PXAction<VATPeriodFilter> checkVATReturn;
		[PXUIField(DisplayName = Messages.RetrieveVATreturn, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable CheckVATReturn(PXAdapter adapter)
		{
			VATApi provider = this.VatProvider;
			if (provider == null)
			{
				throw new PXException(Messages.CouldNotInitApi);
			}

			var headersReturnCurrent = this.headersReturn.Current;
			Model.VATreturn vATreturn = new Model.VATreturn() { periodKey = Period_Header.Current.PeriodKey };

			Guid key = Guid.NewGuid();
			PXLongOperation.StartOperation(key, () =>
			{
				try
				{
					var result = provider.Returns(Period_Header.Current.PeriodKey, headersReturn.Current).GetAwaiter().GetResult();
					PXLongOperation.SetCustomInfo(result, $"Result");
				}
				catch (Exception e)
				{
					PXLongOperation.SetCustomInfo(e, $"Error");
				}
			}
			);
			PXLongOperation.WaitCompletion(key);

			object error = PXLongOperation.GetCustomInfo(key, $"Error");
			if (error is Exception)
			{
				PXTrace.WriteError((Exception)error);
			}
			else
			{
				object customInfo = PXLongOperation.GetCustomInfo(key, $"Result");
				if (customInfo != null)
					vATreturn = (Model.VATreturn)customInfo;
			}

			this.VATRows.Cache.Clear();
			this.VATRows.Insert(new VATRow() { TaxBoxNbr = "1", TaxBoxCode = "vatDueSales", Descr = Messages.vatDueSales, Amt = vATreturn.vatDueSales });
			this.VATRows.Insert(new VATRow() { TaxBoxNbr = "2", TaxBoxCode = "vatDueAcquisitions", Descr = Messages.vatDueAcquisitions, Amt = vATreturn.vatDueAcquisitions });
			this.VATRows.Insert(new VATRow() { TaxBoxNbr = "3", TaxBoxCode = "totalVatDue", Descr = Messages.totalVatDue, Amt = vATreturn.totalVatDue });
			this.VATRows.Insert(new VATRow() { TaxBoxNbr = "4", TaxBoxCode = "vatReclaimedCurrPeriod", Descr = Messages.vatReclaimedCurrPeriod, Amt = vATreturn.vatReclaimedCurrPeriod });
			this.VATRows.Insert(new VATRow() { TaxBoxNbr = "5", TaxBoxCode = "netVatDue", Descr = Messages.netVatDue, Amt = vATreturn.netVatDue });
			this.VATRows.Insert(new VATRow() { TaxBoxNbr = "6", TaxBoxCode = "totalValueSalesExVAT", Descr = Messages.totalValueSalesExVAT, Amt = vATreturn.totalValueSalesExVAT });
			this.VATRows.Insert(new VATRow() { TaxBoxNbr = "7", TaxBoxCode = "totalValuePurchasesExVAT", Descr = Messages.totalValuePurchasesExVAT, Amt = vATreturn.totalValuePurchasesExVAT });
			this.VATRows.Insert(new VATRow() { TaxBoxNbr = "8", TaxBoxCode = "totalValueGoodsSuppliedExVAT", Descr = Messages.totalValueGoodsSuppliedExVAT, Amt = vATreturn.totalValueGoodsSuppliedExVAT });
			this.VATRows.Insert(new VATRow() { TaxBoxNbr = "9", TaxBoxCode = "totalAcquisitionsExVAT", Descr = Messages.totalAcquisitionsExVAT, Amt = vATreturn.totalAcquisitionsExVAT });

			return adapter.Get();
		}
		#endregion

		#region SendVATReturn
		public PXAction<VATPeriodFilter> sendVATreturn;
		[PXUIField(DisplayName = Messages.SubmitVATReturn, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable SendVATReturn(PXAdapter adapter)
		{
			VATPeriodFilter tp = Period_Header.Current;
			if (tp.RevisionId == null) return adapter.Get();
			string error = PXUIFieldAttribute.GetError<VATPeriodFilter.organizationID>(Period_Header.Cache, tp);
			if (string.IsNullOrEmpty(error))
			{
				error = PXUIFieldAttribute.GetError<VATPeriodFilter.branchID>(Period_Header.Cache, tp);
			}
			if (string.IsNullOrEmpty(error) && string.IsNullOrEmpty(tp.PeriodKey))
			{
				error = Messages.TaxPeriodNotFoundHMRC;
			}
			if (!string.IsNullOrEmpty(error))
			{
				WebDialogResult dialog = Period_Header.Ask(Messages.CannotSubmitReturn, PXLocalizer.Localize(error), MessageButtons.OK);
				return adapter.Get();
			}

			WebDialogResult dialogResult = Period_Header.Ask(Messages.VatReturnWillBeSentToHMRC, MessageButtons.YesNoCancel);
			Period_Header.ClearDialog();
			if (dialogResult == WebDialogResult.Cancel) return adapter.Get();

			#region Tax Box
			/*
            Outputs
            Box 1 (vatDueSales) VAT due in the period on sales and other outputs
            Box 2 (vatDueAcquisitions) VAT due in the period on acquisitions from other EU member states
            Box 3 (totalVatDue) Total VAT due (Box 1 + Box 2)

            Inputs
            Box 4 (vatReclaimedCurrPeriod) VAT reclaimed in the period on purchases and other inputs (including acquisitions from the EU)
            Box 5 (netVatDue) net VAT to be paid to HMRC or reclaimed (difference between Box 3 and Box 4)
            Box 6 (totalValueSalesExVAT) total value of sales and all other outputs excluding any VAT
            Box 7 (totalValuePurchasesExVAT) the total value of purchases and all other inputs excluding any VAT
            Box 8 (totalValueGoodsSuppliedExVAT) total value of all supplies of goods and related costs, excluding any VAT, to other EU member states
            Box 9 (totalAcquisitionsExVAT) total value of all acquisitions of goods and related costs, excluding any VAT, from other EU member states
            */
			#endregion

			Model.VATreturn ret = new Model.VATreturn() { periodKey = tp.PeriodKey, finalised = true };

			#region fill report
			decimal amt = 0;
			int lines = 0;
			foreach (PXResult<TaxReportLine, TaxHistoryReleased> res in this.Period_Details.Select())
			{
				TaxReportLine line = res;
				TaxHistoryReleased hist = res;
				amt = hist.ReportFiledAmt ?? 0;
				switch (line.ReportLineNbr)
				{
					case "1": ret.vatDueSales = amt; break;
					case "2": ret.vatDueAcquisitions = amt; break;
					case "3": ret.totalVatDue = amt; break;
					case "4": ret.vatReclaimedCurrPeriod = amt; break;
					case "5": ret.netVatDue = amt; break;
					case "6": ret.totalValueSalesExVAT = (long)amt; break;
					case "7": ret.totalValuePurchasesExVAT = (long)amt; break;
					case "8": ret.totalValueGoodsSuppliedExVAT = (long)amt; break;
					case "9": ret.totalAcquisitionsExVAT = (long)amt; break;
				}
				if (line.ReportLineNbr != null)
				{
					lines++;
				}
			}

			if (lines == 0)
			{
				throw new PXException(Messages.NoReportLinesToSend);
			}
			#endregion

			VATApi provider = this.VatProvider;
			if (provider == null)
			{
				PXTrace.WriteError(Messages.CouldNotInitApi);
				throw new PXException(Messages.CouldNotInitApi);
			}

			Guid key = Guid.NewGuid();
			PXLongOperation.StartOperation(key, () =>
			{
				try
				{
					var result = provider.SendReturn(ret, headersReturn.Current).GetAwaiter().GetResult();
					PXLongOperation.SetCustomInfo(result, $"Result");
				}
				catch (Exception e)
				{
					PXLongOperation.SetCustomInfo(e, $"Error");
				}
			}
			);
			PXLongOperation.WaitCompletion(key);

			object er = PXLongOperation.GetCustomInfo(key, $"Error");
			if (er is Exception)
			{
				PXTrace.WriteError((Exception)er);
				Exception e = (Exception)er;
				throw new PXException(e.Message);
			}
			else
			{
				object customInfo = PXLongOperation.GetCustomInfo(key, $"Result");
				if (customInfo != null)
				{
					if (customInfo is VaTreturnResponse)
					{
						throw new PXException(Messages.VATreturnIsAccepted);
					}
				}
			}

			return adapter.Get();
		}
		#endregion

		#region viewTaxDocument
		public PXAction<VATPeriodFilter> viewTaxDocument;
		[PXUIField(DisplayName = PX.Objects.TX.Messages.ViewDocuments, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewTaxDocument(PXAdapter adapter)
		{
			if (this.Period_Details.Current != null)
			{
				ReportTaxDetail graph = PXGraph.CreateInstance<ReportTaxDetail>();
				TaxHistoryMaster filter = PXCache<TaxHistoryMaster>.CreateCopy(graph.History_Header.Current);
				filter.OrganizationID = Period_Header.Current.OrganizationID;
				filter.BranchID = Period_Header.Current.BranchID;
				filter.VendorID = Period_Header.Current.VendorID;
				filter.TaxPeriodID = Period_Header.Current.TaxPeriodID;
				filter.LineNbr = Period_Details.Current.LineNbr;
				graph.History_Header.Update(filter);
				throw new PXRedirectRequiredException(graph, PX.Objects.TX.Messages.ViewDocuments);
			}
			return Period_Header.Select();
		}
		#endregion

		#endregion

		public ConnectionInfo GetConnectionInfo()
		{
			VATPeriodFilter p = Period_Header.Current;
			ConnectionInfo info = new ConnectionInfo();
			if (p?.OrganizationID == null) return info;

			OrganizationTaxInfo org = null;
			org =
				PXSelect<
						OrganizationTaxInfo,
						Where<OrganizationTaxInfo.organizationID, Equal<Required<OrganizationTaxInfo.organizationID>>>>
					.SelectSingleBound(this, null, p.OrganizationID)
					.FirstOrDefault();

			BranchTaxInfo branch = null;
			if (org.FileTaxesByBranches == true && p.BranchID != null)
			{
				branch =
					PXSelect<
							BranchTaxInfo,
							Where<BranchTaxInfo.branchID, Equal<Required<BranchTaxInfo.branchID>>>>
						.SelectSingleBound(this, null, p.BranchID)
						.FirstOrDefault();
			}

			int? appId = null;
			if (branch != null)
			{
				appId = branch.ApplicationID;
				info.Vrn = branch.TaxRegistrationID;

			}
			else if (org != null)
			{
				appId = org.ApplicationID;
				info.Vrn = org.TaxRegistrationID;
			}

			if (appId != null)
			{
				info.Application = SelectFrom<HMRCOAuthApplication>
					.Where<HMRCOAuthApplication.applicationID.IsEqual<@P.AsInt>>
					.View.Select(this, appId);
				info.Token = SelectFrom<OAuthToken>
					.Where<OAuthToken.applicationID.IsEqual<@P.AsInt>>
					.View.Select(this, info.Application.ApplicationID);
				return info;
			}

			return null;
		}

		public virtual void UpdateGovHeadersData(PXGraph graph, string payload)
		{
			PX.SM.Licensing license = PXViewOf<PX.SM.Licensing>.Select(graph).FirstOrDefault();
			string licenseKey = license?.LicensingKey;

			PX.SM.Version version = PXSelect<PX.SM.Version>.Select(graph);

			Struct.Root document = JsonConvert.DeserializeObject<Struct.Root>(payload);
			HMRCHeaderData row = new HMRCHeaderData();

			row.GovClientConnectionMethod = "WEB_APP_VIA_SERVER";
			row.GovClientPublicIP = document.clientPublicIP;
			row.GovClientPublicIPTimestamp = Helper.GetUtcTimestamp();
			row.GovClientPublicPort = HttpContext.Current.Request.Params["REMOTE_PORT"] ?? "";
			row.GovClientDeviceID = Helper.GetDeviceId();
			row.GovClientUserIDs = string.Format("Acumatica={0}", HttpUtility.UrlEncode(graph.Accessinfo.UserName));
			row.GovClientTimezone = Helper.GetTimeZone();
			row.GovClientLocalIPs = Helper.GetLocalIPAddresses();
			row.GovClientLocalIPsTimestamp = Helper.GetUtcTimestamp();
			row.GovClientScreens = document.clientScreens;
			row.GovClientWindowSize = document.windowSize;
			row.GovClientBrowserPlugins = document.clientBrowserPlugins;
			row.GovClientBrowserJSUserAgent = HttpContext.Current.Request.UserAgent;
			row.GovClientBrowserDoNotTrack = "false";
			row.GovClientMultiFactor = "";
			row.GovVendorProductName = Uri.EscapeDataString("Acumatica ERP");
			row.GovVendorVersion = $"AcumaticaJS={Uri.EscapeDataString(version.CurrentVersion)}&AcumaticaERP={Uri.EscapeDataString(version.CurrentVersion)}";
			row.GovVendorLicenseIDs = $"Acumatica={Helper.GetLicenseHash(licenseKey)}";
			row.GovVendorPublicIP = Helper.GetVendorIPAddress();
			row.GovVendorForwarded = "by=" + row.GovVendorPublicIP + "&for=" + document.clientPublicIP;

			if (headersReturn.Current == null)
				headersReturn.Insert(row);
			else
				headersReturn.Update(row);
		}

		public PXSelect<HMRCHeaderData> headersReturn;

		#region Internal Types

		public class Struct
		{
			public class Root
			{
				public string clientBrowserPlugins { get; set; }
				public string clientScreens { get; set; }
				public string windowSize { get; set; }
				public string clientPublicIP { get; set; }
			}
		}

		#endregion
	}
}
