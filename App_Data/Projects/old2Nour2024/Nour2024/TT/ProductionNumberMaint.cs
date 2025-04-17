using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CS;

namespace Maintenance
{
    public class ProductionNumberMaint : PXGraph<ProductionNumberMaint>
    {
        public PXSave<ProductionNumber> Save;
        public PXCancel<ProductionNumber> Cancel;
        public PXInsert<ProductionNumber> Insert;
        public PXDelete<ProductionNumber> Delete;
        public PXFirst<ProductionNumber> First;
        public PXPrevious<ProductionNumber> Previous;
        public PXNext<ProductionNumber> Next;
        public PXLast<ProductionNumber> Last;



        public PXSelect<ProductionNumber> productionNbr;

        protected void ProductionNumber_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            var row = (ProductionNumber)e.Row;
            if (string.IsNullOrEmpty(row.RefNbr) == true)
                row.RefNbr = AutoNumberAttribute.GetNextNumber(sender, row, "PRODUCTION", DateTime.Today);
        }


        public PXAction<ProductionNumber> report;
        [PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
        [PXButton]
        protected IEnumerable Report(PXAdapter adapter,
          [PXString(8)]
    [PXStringList(new string[] { "TT302001" }, new string[] { "Production Number Details" })]
    string reportID
    )
        {
            foreach (ProductionNumber doc in adapter.Get<ProductionNumber>())
            {
                switch (reportID)
                {
                    case "TT302001":
                        {
                            Dictionary<string, string> parameters = new Dictionary<string, string>();
                            parameters["RefNbr"] = doc.RefNbr;
                            throw new PXReportRequiredException(parameters, reportID, "Report");
                        }
                }
            }
            return adapter.Get();
        }
    }
}