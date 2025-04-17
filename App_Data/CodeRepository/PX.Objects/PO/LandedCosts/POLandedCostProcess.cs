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
using PX.Data;
using PX.Objects.AP;
using PX.Objects.AP.MigrationMode;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[PX.Objects.GL.TableAndChartDashboardType]
	[Serializable]
	public class POLandedCostProcess : PXGraph<POLandedCostProcess>
	{

		public PXCancel<POLandedCostDoc> Cancel;
		public PXAction<POLandedCostDoc> ViewDocument;

		[PXFilterable]
		public PXProcessingJoin<POLandedCostDoc, InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POLandedCostDoc.vendorID>>>,
			Where<POLandedCostDoc.released, Equal<False>, And<POLandedCostDoc.hold, Equal<False>,
			And<Match<Vendor, Current<AccessInfo.userName>>>>>> landedCostDocsList;


		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton()]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			if (this.landedCostDocsList.Current != null)
			{
				var graph = PXGraph.CreateInstance<POLandedCostDocEntry>();
				var poDoc = graph.Document.Search<POLandedCostDoc.refNbr>(landedCostDocsList.Current.RefNbr, landedCostDocsList.Current.DocType);
				if (poDoc != null)
				{
					graph.Document.Current = poDoc;
					throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}
			return adapter.Get();
		}

		public PXSetup<POSetup> POSetup;

		public POLandedCostProcess()
		{
			APSetupNoMigrationMode.EnsureMigrationModeDisabled(this);
			POSetup setup = POSetup.Current;

			landedCostDocsList.SetSelected<POLandedCostDoc.selected>();
			landedCostDocsList.SetProcessCaption(Messages.Process);
			landedCostDocsList.SetProcessAllCaption(Messages.ProcessAll);
			landedCostDocsList.SetProcessDelegate(delegate (List<POLandedCostDoc> list)
			{
				ReleaseDoc(list, true);
			});
		}

		public static void ReleaseDoc(List<POLandedCostDoc> list, bool aIsMassProcess)
		{
			var docgraph = PXGraph.CreateInstance<POLandedCostDocEntry>();
			int iRow = 0;
			bool failed = false;
			foreach (POLandedCostDoc doc in list)
			{
				try
				{
					docgraph.ReleaseDoc(doc);
					PXProcessing<POLandedCostDoc>.SetInfo(iRow, ActionsMessages.RecordProcessed);
				}
				catch (Exception e)
				{
					if (aIsMassProcess)
					{
						PXProcessing<POLandedCostDoc>.SetError(iRow, e);
						failed = true;
					}
					else
						throw;
				}
				iRow++;
			}
			if (failed)
			{
				throw new PXException(Messages.LandedCostProcessingForOneOrMorePOReceiptsFailed);
			}
		}
	}
}
