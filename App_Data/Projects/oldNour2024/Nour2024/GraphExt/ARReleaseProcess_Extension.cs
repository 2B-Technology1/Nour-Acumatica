using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using PX.Data;
using PX.Common;
using PX.Objects.AR.BQL;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.CA;
using PX.Objects.DR;
using PX.Objects.CR;
using PX.Objects.SO;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.Overrides.ARDocumentRelease;
using PX.Objects.Common;
using PX.Objects.Common.DataIntegrity;
//using Avalara.AvaTax.Adapter;
//using Avalara.AvaTax.Adapter.TaxService;
using SOOrder = PX.Objects.SO.SOOrder;
using SOInvoice = PX.Objects.SO.SOInvoice;
using SOOrderShipment = PX.Objects.SO.SOOrderShipment;
using INTran = PX.Objects.IN.INTran;
using PMTran = PX.Objects.PM.PMTran;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects;
using PX.Objects.AR;

namespace PX.Objects.AR
{

    public class ARReleaseProcess_Extension : PXGraphExtension<ARReleaseProcess>
    {

		////View to save data
		////public PXSelect<AACustomTable> ViewForCustomTable;

		//#region Event Handlers
		////Overriding Saving method to add our logic
		//public delegate void PersistDelegate();
		//[PXOverride]
		//public void Persist(PersistDelegate baseMethod)
		//{
		//	baseMethod();

		//	ARRegister doc =new ARRegister();

		//	//Check for document and released flag
		//	if (doc != null && doc.Released == true &&
		//		//Check for doc type.
		//		(doc.DocType == ARDocType.CreditMemo)
		//	{
		//		using (PXTransactionScope ts = new PXTransactionScope())
		//		{
		//			AACustomTable row = ViewForCustomTable.Insert();
		//			row.TableValue = DateTime.Now.ToString();
		//			ViewForCustomTable.Update(row);

		//			//Manually Saving as base code will not call base graph persis.
		//			ViewForCustomTable.Cache.Persist(PXDBOperation.Insert);
		//			ViewForCustomTable.Cache.Persist(PXDBOperation.Update);

		//			ts.Complete(Base);
		//		}
		//	}

		//	//Triggering after save events.
		//	ViewForCustomTable.Cache.Persisted(false);
		//}
		//#endregion

	}


}