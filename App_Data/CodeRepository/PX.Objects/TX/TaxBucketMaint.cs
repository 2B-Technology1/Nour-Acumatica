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

using PX.Common;
using PX.Data;

using PX.Objects.Common;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.TX.Descriptor;
using PX.Objects.Common.Scopes;

namespace PX.Objects.TX
{
	public class TaxType : ILabelProvider
	{
		private static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
		{
			{ Sales, Messages.Output },
			{ Purchase, Messages.Input },
			{ Recognition, Messages.Recognition },
		};
		
		public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;

		public const string Sales = "S";
        public const string Purchase = "P";
        public const string PendingSales = "A";
        public const string PendingPurchase = "B";
		public const string Recognition = "Y";

		public class sales : PX.Data.BQL.BqlString.Constant<sales>
		{
            public sales() : base(Sales) { ;}
        }

        public class purchase : PX.Data.BQL.BqlString.Constant<purchase>
		{
            public purchase() : base(Purchase) { ;}
        }

        public class pendingSales : PX.Data.BQL.BqlString.Constant<pendingSales>
		{
            public pendingSales() : base(PendingSales) { ;}
        }

        public class pendingPurchase : PX.Data.BQL.BqlString.Constant<pendingPurchase>
		{
            public pendingPurchase() : base(PendingPurchase) { ;}
        }

		public class recognition : PX.Data.BQL.BqlString.Constant<recognition>
		{
			public recognition() : base(Recognition) {; }
		}
	}

    [System.SerializableAttribute()]
    public partial class TaxBucketMaster : IBqlTable
    {
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected Int32? _VendorID;
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [TaxAgencyActive]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #region BucketID
        public abstract class bucketID : PX.Data.BQL.BqlInt.Field<bucketID>
		{
            public class TaxBucketSelectorAttribute : PXSelectorAttribute
            {
                public TaxBucketSelectorAttribute()
                    : base(typeof(Search<TaxBucket.bucketID,
                        Where<TaxBucket.vendorID, Equal<Current<TaxBucketMaster.vendorID>>>>))
                {
                    DescriptionField = typeof(TaxBucket.name);
                    _UnconditionalSelect = new Search<TaxBucket.bucketID,
                        Where<TaxBucket.vendorID, Equal<Current<TaxBucketMaster.vendorID>>,
                            And<TaxBucket.bucketID, Equal<Required<TaxBucket.bucketID>>>>>();
                }
            }
        }
        protected Int32? _BucketID;
        [PXDBInt()]
        //[PXIntList(new int[] { 0 }, new string[] { "undefined" })]
        [bucketID.TaxBucketSelector]
        [PXUIField(DisplayName = "Reporting Group", Visibility = PXUIVisibility.Visible, Required = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual Int32? BucketID
        {
            get
            {
                return this._BucketID;
            }
            set
            {
                this._BucketID = value;
            }
        }
		#endregion
		#region TaxReportRevisionID
		public abstract class taxReportRevisionID : PX.Data.BQL.BqlInt.Field<taxReportRevisionID> { }
        [PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Report Version", Required = true)]
		[PXSelector(typeof(Search<TaxReport.revisionID, Where<TaxReport.vendorID, Equal<Current<TaxBucketMaster.vendorID>>>, OrderBy<Desc<TaxReport.revisionID>>>),
			new Type[] { typeof(TaxReport.revisionID), typeof(TaxReport.validFrom) })]
		public virtual int? TaxReportRevisionID { get; set; }
        #endregion
        #region BucketType
        public abstract class bucketType : PX.Data.BQL.BqlString.Field<bucketType> { }
        protected String _BucketType;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(TaxType.Sales)]
        [PXUIField(DisplayName = "Group Type", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [LabelList(typeof(TaxType))]
        public virtual String BucketType
        {
            get
            {
                return this._BucketType;
            }
            set
            {
                this._BucketType = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        protected String _Descr;
        [PXDBString(60, IsUnicode = true)]
        public virtual String Descr
        {
            get
            {
                return this._Descr;
            }
            set
            {
                this._Descr = value;
            }
        }
        #endregion
    }

    public class TaxBucketMaint : PXGraph<TaxBucketMaint>
    {
        public PXSave<TaxBucketMaster> Save;
        public PXCancel<TaxBucketMaster> Cancel;

        public PXFilter<TaxBucketMaster> Bucket;
        public PXSelectJoin<TaxBucketLine, 
        	InnerJoin<TaxReportLine, 
        		On<TaxReportLine.vendorID, Equal<TaxBucketLine.vendorID>, 
                    And<TaxReportLine.taxReportRevisionID, Equal<TaxBucketLine.taxReportRevisionID>,
        			And<TaxReportLine.lineNbr, Equal<TaxBucketLine.lineNbr>>>>>,
        	Where<TaxBucketLine.vendorID, Equal<Current<TaxBucketMaster.vendorID>>,
        		And<TaxBucketLine.bucketID, Equal<Current<TaxBucketMaster.bucketID>>,
				And<TaxBucketLine.taxReportRevisionID, Equal<Current<TaxBucketMaster.taxReportRevisionID>>,
        		And<TaxReportLine.tempLineNbr, IsNull>>>>,
			OrderBy<
				Asc<TaxReportLine.sortOrder>>> BucketLine;

	    public PXSelect<TaxReportLine> ReportLineView;

    	public PXSetup<Vendor, Where<Vendor.bAccountID, Equal<Current<TaxBucketMaster.vendorID>>>> Vendor; 
       
        public TaxBucketMaint()
        {
		    FieldDefaulting.AddHandler<BAccountR.type>((sender, e) => { if (e.Row != null) e.NewValue = BAccountType.VendorType; });
        }

        private void ValidateBucket()
        {
            TaxBucketMaster currBucket = this.Bucket.Current;
			TaxReportMaint.TaxBucketAnalizer TestAnalyzerTax = new TaxReportMaint.TaxBucketAnalizer(this, (int)currBucket.VendorID, TaxReportLineType.TaxAmount, (int)currBucket.TaxReportRevisionID);
			TestAnalyzerTax.DoChecks((int)currBucket.BucketID);
			TaxReportMaint.TaxBucketAnalizer TestAnalyzerTaxable = new TaxReportMaint.TaxBucketAnalizer(this, (int)currBucket.VendorID, TaxReportLineType.TaxableAmount, (int)currBucket.TaxReportRevisionID);
			TestAnalyzerTaxable.DoChecks((int)currBucket.BucketID);
        }

		public override void Persist()
        {
			if (Bucket.VerifyFullyValid())
			{
				ValidateBucket();
				TaxReportMaint.CheckReportSettingsEditable(this, Bucket.Current.VendorID, Bucket.Current.TaxReportRevisionID);

				base.Persist();
			}
        }
		public class SuppressCascadeScope : FlaggedModeScopeBase<SuppressCascadeScope> { }

		protected virtual void TaxBucketMaster_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
	    {
			TaxBucketMaster taxBucket = e.Row as TaxBucketMaster;

		    if (taxBucket != null)
			{
				PXFieldState bAccIDfieldState = cache.GetStateExt<TaxBucketMaster.taxReportRevisionID>(e.Row) as PXFieldState;
				if (bAccIDfieldState != null && bAccIDfieldState.ErrorLevel != PXErrorLevel.Error)
				{
					cache.RaiseExceptionHandling<TaxBucketMaster.taxReportRevisionID>(e.Row, bAccIDfieldState.Value, null);
					TaxReportMaint.CheckReportSettingsEditableAndSetWarningTo<TaxBucketMaster.taxReportRevisionID>(this, cache, taxBucket,
						taxBucket.VendorID, taxBucket.TaxReportRevisionID);
				}
			}
	    }
		protected virtual void _(Events.FieldUpdated<TaxBucketMaster.vendorID> e)
		{
			if (e.Row == null) return;
			TaxBucketMaster row = e.Row as TaxBucketMaster;

			if (!this.IsImport)
			{
				e.Cache.SetValue<TaxBucketMaster.taxReportRevisionID>(e.Row, null);
			}
		}

        protected virtual void TaxBucketLine_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
			if (SuppressCascadeScope.IsActive == false)
			{
				using (new SuppressCascadeScope())
				{
					foreach (PXResult<TaxReportLine, TaxBucketLine> res in PXSelectJoin<
						TaxReportLine,
						LeftJoin<TaxBucketLine,
							On<TaxBucketLine.vendorID, Equal<TaxReportLine.vendorID>,
							And<TaxBucketLine.taxReportRevisionID, Equal<TaxReportLine.taxReportRevisionID>,
							And<TaxBucketLine.lineNbr, Equal<TaxReportLine.lineNbr>>>>>,
						Where<TaxReportLine.vendorID, Equal<Required<TaxReportLine.vendorID>>,
							And<TaxReportLine.taxReportRevisionID, Equal<Required<TaxReportLine.taxReportRevisionID>>,
							And<TaxReportLine.tempLineNbr, Equal<Required<TaxReportLine.tempLineNbr>>,
							And<TaxBucketLine.bucketID, Equal<Required<TaxBucketLine.bucketID>>>>>>>
						.Select(this, ((TaxBucketLine)e.Row).VendorID, ((TaxBucketLine)e.Row).TaxReportRevisionID, ((TaxBucketLine)e.Row).LineNbr, ((TaxBucketLine)e.Row).BucketID))
					{
						if (((TaxBucketLine)res).BucketID != null)
						{

							BucketLine.Cache.Delete((TaxBucketLine)res);

						}
					}
				}

			}

        }



		protected virtual void TaxBucketLine_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (SuppressCascadeScope.IsActive == false)
			{
				using (new SuppressCascadeScope())
				{
					foreach (PXResult<TaxReportLine> res in PXSelect<
					TaxReportLine,
					Where<TaxReportLine.vendorID, Equal<Required<TaxReportLine.vendorID>>,
						And<TaxReportLine.taxReportRevisionID, Equal<Required<TaxReportLine.taxReportRevisionID>>,
						And<TaxReportLine.tempLineNbr, Equal<Required<TaxReportLine.tempLineNbr>>>>>>
					.Select(this, ((TaxBucketLine)e.Row).VendorID, ((TaxBucketLine)e.Row).TaxReportRevisionID, ((TaxBucketLine)e.Row).LineNbr))
					{

						TaxBucketLine new_bucket = PXCache<TaxBucketLine>.CreateCopy((TaxBucketLine)e.Row);
						new_bucket.LineNbr = ((TaxReportLine)res).LineNbr;
						new_bucket.TaxReportRevisionID = ((TaxReportLine)res).TaxReportRevisionID;
						BucketLine.Cache.Insert(new_bucket);
					}

					BucketLine.Cache.Current = e.Row;
				}
			}

		}

		protected virtual void TaxBucketLine_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			var bucketLine = (TaxBucketLine)e.Row;

			var reportLine = new TaxReportLine()
			{
				VendorID = bucketLine.VendorID,
				TaxReportRevisionID = bucketLine.TaxReportRevisionID,
				LineNbr = bucketLine.LineNbr
			};

			reportLine = (TaxReportLine)ReportLineView.Cache.Locate(reportLine);

			if (reportLine?.TempLineNbr == null)//it is parent line
			{
				CheckUnique(cache, bucketLine);
			}
		}

		protected virtual void TaxBucketLine_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var bucketLine = e.Row as TaxBucketLine;

            if (bucketLine == null)
				return;

            PXUIFieldAttribute.SetReadOnly<TaxBucketLine.lineNbr>(cache, bucketLine, bucketLine.LineNbr != null);
        }

        protected void CheckUnique(PXCache cache, TaxBucketLine bucketLine)
	    {
			var dataRowWithSameLineNbr = PXSelectJoin<TaxBucketLine,
														InnerJoin<TaxReportLine,
															On<TaxReportLine.vendorID, Equal<TaxBucketLine.vendorID>,
                                                                And<TaxReportLine.taxReportRevisionID, Equal<TaxBucketLine.taxReportRevisionID>,
																And<TaxReportLine.lineNbr, Equal<TaxBucketLine.lineNbr>>>>>,
														Where<TaxBucketLine.vendorID, Equal<Current<TaxBucketMaster.vendorID>>,
															And<TaxBucketLine.bucketID, Equal<Current<TaxBucketMaster.bucketID>>,
															And<TaxReportLine.tempLineNbr, IsNull,
                                                            And<TaxBucketLine.taxReportRevisionID, Equal<Required<TaxBucketLine.taxReportRevisionID>>,
															And<TaxBucketLine.lineNbr, Equal<Required<TaxBucketLine.lineNbr>>>>>>>>
														.Select(this, bucketLine.TaxReportRevisionID, bucketLine.LineNbr).AsEnumerable()
														.Cast<PXResult<TaxBucketLine, TaxReportLine>>()
														.ToArray();

			if (dataRowWithSameLineNbr.Any())
			{
				var reportLine = (TaxReportLine)dataRowWithSameLineNbr.First();

				var bucket = (TaxBucket)PXSelect<TaxBucket,
											Where<TaxBucket.vendorID, Equal<Current<TaxBucketMaster.vendorID>>,
												And<TaxBucket.bucketID, Equal<Current<TaxBucketMaster.bucketID>>>>>
											.Select(this);

                cache.RaiseExceptionHandling<TaxBucketLine.lineNbr>(bucketLine, bucketLine.LineNbr,
                    new PXSetPropertyException(Messages.TheReportingGroupAlreadyContainsTheReportLine, bucket.Name, reportLine.Descr));
                throw new PXException(Messages.TheReportingGroupAlreadyContainsTheReportLine, bucket.Name,
					reportLine.Descr);
            }
	    }

        protected virtual void TaxBucketMaster_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            TaxBucket bucket = (TaxBucket)PXSelect<TaxBucket, Where<TaxBucket.vendorID, Equal<Required<TaxBucket.vendorID>>, And<TaxBucket.bucketID, Equal<Required<TaxBucket.bucketID>>>>>.Select(this, ((TaxBucketMaster)e.Row).VendorID, ((TaxBucketMaster)e.Row).BucketID);

            if (bucket != null)
            {
                ((TaxBucketMaster)e.Row).BucketType = bucket.BucketType;
            }
        }

        public PXSetup<APSetup> APSetup;
    }
}
