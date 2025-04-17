using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PX.Common;
using PX.Data;
using System.Collections;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR.CRCaseMaint_Extensions;
using PX.Objects.CR.Extensions;
using PX.Objects.CT;
using PX.Objects.CR.Workflows;
using PX.Objects.GL;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.SM;
using PX.TM;
using PX.Objects;
using PX.Objects.CR;
using Nour2024.Helpers;
using Nour2024.Models;

using PX.Objects.SO;
using PX.Data.BQL;
using Nour2024.DAC;
using PX.Common.Mail;
using PX.Data.BusinessProcess;
using Newtonsoft.Json;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CR
{
    public class CRCaseMaint_Extension : PXGraphExtension<PX.Objects.CR.CRCaseMaint>
    {
        //public PXFilter<Email> EmailView;

        public PXAction<CRCase> SendSMS;
        [PXButton(Connotation = Data.WorkflowAPI.ActionConnotation.Warning)]

        [PXUIField(DisplayName = "SendSMS", Enabled = true)]
        protected virtual IEnumerable sendSMS(PXAdapter adapter)
        {
            CRCase current = Base.Case.Current;
            string ticketID = Base.Case.Current?.CaseCD;
            Contact contact = SelectFrom<Contact>.Where<Contact.bAccountID.IsEqual<@P.AsInt>>.View.Select(Base, current?.CustomerID);
            string recieverNbr = contact?.Phone1;
            if (String.IsNullOrEmpty(recieverNbr))
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException("Phone Number is missing\nplease make sure you inserted the phone number");

            string smsMessage = $" „  ”ÃÌ· ÿ·»ﬂ„  Õ  —ﬁ„ : {ticketID} Ê ”Ì „ «·›Õ’ Ê «·—œ Œ·«· 48 ”«⁄…, Ê Ì„ﬂ‰ﬂ„ «·„ «»⁄… „‰ Œ·«· «·« ’«· ⁄·Ì 19943 ";
            string response = SmsSender.SendMessage(smsMessage, recieverNbr);
            salssmsrespons responseRoot = JsonConvert.DeserializeObject<salssmsrespons>(response);
            if (responseRoot != null)
            {
                if (responseRoot.message == "success")
                {
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Message sent successfully");
                }
            }
            return adapter.Get();
        }
        //protected void _(Events.RowSelected<CRCase> e)
        //{
        //    var row = e.Row;
        //    if (row == null) return;

        //    if (row.Status == "C")
        //    {
        //        row.ResolutionDate = PXTimeZoneInfo.Now;
        //        Base.Case.Update(row);
        //    }
        //}

        //protected void _(Events.RowSelected<CRCase> e)
        //{
        //    var row = e.Row;
        //    if (row == null) return;

        //    var usrCaseStatus = row.GetExtension<crcase_ex>().UsrCaseStatus;

        //    if (usrCaseStatus == "C")
        //    {
        //        row.ResolutionDate = PXTimeZoneInfo.Now;
        //        Base.Case.Update(row);
        //    }
        //}

        //protected virtual void _(Events.RowUpdated<CRCase> e)

        //{


        //    //var row = (CRCase)e.Row;
        //    //if (row == null) return;


        //    //if (row.Status == "Closed")
        //    //{
        //    //    row.ResolutionDate = PXTimeZoneInfo.Now;
        //    //    //Base.Case.Update(row);
        //    //    e.Cache.Update(row);
        //    //}
        //    var row = e.Row;
        //    if (row == null) return;

        //    if (row.Status == "C")
        //    {
        //        row.ResolutionDate = PXTimeZoneInfo.Now;
        //        //e.Cache.Update(row);
        //        Base.Case.Update(row);
        //    }

        //}
        //protected virtual void _(Events.RowUpdated<CRCase> e)
        //{
        //    var row = e.Row;
        //    if (row == null) return;

        //    if (row.Status == "Closed")
        //    {
        //        row.ResolutionDate = PXTimeZoneInfo.Now;
        //        e.Cache.Update(row);
        //    }
        //}


        //protected void _(Events.FieldUpdated<CRCase, CRCase.status> e)

        ////protected virtual void CRCase_Status_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        //{
        //    //if (InvokeBaseHandler != null)
        //    //    InvokeBaseHandler(cache, e);
        //    var row = (CRCase)e.Row;
        //    if (row == null) return;


        //    if (row.Status == "Closed")
        //    {
        //        row.ResolutionDate = PXTimeZoneInfo.Now;
        //        Base.Case.Update(row);
        //    }

        //}
    }





}