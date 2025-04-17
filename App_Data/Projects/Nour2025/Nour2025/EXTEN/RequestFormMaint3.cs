using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR;
using MyMaintaince;

namespace NourSc202007071
{
    public class RequestFormMaint3 : PXGraph<RequestFormMaint3, RequestForm>
    {
        public PXSelect<RequestForm> requestForm;
        public PXSelect<RequestFormDetail, Where<RequestFormDetail.refNbr, Equal<Current<RequestForm.refNbr>>>> requestFormDet;
       



        protected void RequestForm_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            try
            {
                if (this.requestForm.Current.RefNbr == "<NEW>")
                {
                    SetUp result = PXSelect<SetUp, Where<SetUp.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
                    string lastNumber = result.RequstRefNbr;
                    char[] symbols = lastNumber.ToCharArray();
                    for (int i = symbols.Length - 1; i >= 0; i--)
                    {
                        if (!char.IsDigit(symbols[i]))
                            break;
                        if (symbols[i] < '9')
                        {
                            symbols[i]++;
                            break;
                        }
                        symbols[i] = '0';
                    }
                    this.requestForm.Current.RefNbr = new string(symbols);
                    this.ProviderUpdate<SetUp>(new PXDataFieldAssign("RequstRefNbr", new string(symbols)), new PXDataFieldRestrict("branchID", this.Accessinfo.BranchID));

                }
                if (this.requestForm.Current.Status == "Closed")
                {
                    this.requestForm.Current.ClosedDateTime = DateTime.Now;
                //    PXUIFieldAttribute.SetEnabled(requestForm.Cache, null, false);
                //    PXUIFieldAttribute.SetEnabled(requestFormDet.Cache, null, false);
                }
            }
            catch
            { }

        }
        protected void RequestForm_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {

            if (e.Row != null)
            {
                if (e.TranStatus != PXTranStatus.Aborted)
                {
                    if (this.requestForm.Current.Status == "Closed")
                    {
                        this.requestForm.Current.ClosedDateTime = DateTime.Now;
                        this.requestForm.Current.ClosedBy = Accessinfo.UserName;
                        PXUIFieldAttribute.SetEnabled(requestForm.Cache, null, false);
                        PXUIFieldAttribute.SetEnabled(requestFormDet.Cache, null, false);

                    }
                }
            }

        }
                
                

        public virtual void RequestForm_customer_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            //RequestForm row = e.Row as RequestForm;
            //if (this.requestForm.Current.RefNbr != "<NEW>")
            //{
            //    Customer c = PXSelect<Customer, Where<Customer.acctCD, Equal<Required<Customer.acctCD>>>>.Select(this, e.NewValue);
            //    this.requestForm.Current.Name = c.AcctName;
            //}

        }
        public virtual void RequestForm_usrWarrantyStatus_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            RequestForm row = e.Row as RequestForm;
            if (this.requestForm.Current.RefNbr != "<NEW>")
            {
                if (row.UsrWarrantyStatus != null)
                {
                    row.UsrApprovalDate = DateTime.Now;
                    row.Approvedby = Accessinfo.UserName;
                    Actions.PressSave();
                }
            }

        }
        public virtual void RequestForm_UsrPartsStatus_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            RequestForm row = e.Row as RequestForm;
            if (this.requestForm.Current.RefNbr != "<NEW>")
            {
                if (row.UsrPartsStatus != null)
                {
                    row.PartsApprovalDate = DateTime.Now;
                    row.PartsApprovedby = Accessinfo.UserName;
                    Actions.PressSave();
                }
            }

        }


        public virtual void RequestForm_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
            RequestForm row = e.Row as RequestForm;

            if (e.Row != null)
            {
                if (row.Status == "Closed")
                {
                    PXUIFieldAttribute.SetEnabled(requestForm.Cache, null, false);
                    PXUIFieldAttribute.SetEnabled(requestFormDet.Cache, null, false);

                }
                else
                {
                    PXUIFieldAttribute.SetEnabled(requestForm.Cache, null, true);
                    PXUIFieldAttribute.SetEnabled(requestFormDet.Cache, null, true);

                }


            }


        }
    }
}