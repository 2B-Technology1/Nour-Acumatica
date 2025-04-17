using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.CR.Extensions.CRDuplicateEntities;
using PX.Objects.CR.Extensions.SideBySideComparison;
using PX.Objects.CR.Extensions.SideBySideComparison.Merge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects;
using PX.Objects.CR;
using System.Collections;
using PX.Objects.SO;
using Nour2024.Helpers;
using Nour2024.Models;
using Newtonsoft.Json;
using PX.TM;
using PX.Objects.CR.MassProcess;



namespace PX.Objects.CR
{
  public class LeadMaint_Extension : PXGraphExtension<PX.Objects.CR.LeadMaint>
  {
        public PXAction<CRLead> SendSMS;
        [PXButton(Connotation = Data.WorkflowAPI.ActionConnotation.Warning)]
        [PXUIField(DisplayName = "SendSMS", Enabled = true)]
        protected virtual IEnumerable sendSMS(PXAdapter adapter)
        {
            CRLead current = Base.Lead.Current;
            if (current.ContactID == null)
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException("You have to save first");

            string recieverNbr = Base.LeadCurrent.Current?.Phone1;
            if(!String.IsNullOrEmpty(recieverNbr))
            {
                string smsMessage = $"⁄„Ì·‰« «·⁄“Ì“  ‰‘ﬂ—ﬂ„ ⁄·Ì  Ê«’·ﬂ„ „⁄‰« Ê”Ì „ «·—œ ⁄·Ì «” ›”«—ﬂ„ ›Ì «ﬁ—» Êﬁ  „„ﬂ‰ Ê Ì„ﬂ‰ﬂ„  ··«” ›”«— Ê«·„ «»⁄… „‰ Œ·«· «·« ’«· ⁄·Ì 19943.";
                string response = SmsSender.SendMessage(smsMessage, recieverNbr);
                salssmsrespons responseRoot = JsonConvert.DeserializeObject<salssmsrespons>(response);
                if (responseRoot.message == "success")
                {
                    // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                    throw new PXException("Message sent successfully");
                }
            }
            else
            {
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException("Phone Number is missing\nplease make sure you inserted the phone number");
            }
            return adapter.Get();
        }

       
        #region Cache attach events to make Mandatory fields
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault]
        [PXUIField(Required = true)]
        protected virtual void _(Events.CacheAttached<CRLead.ownerID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault]
        [PXUIField(Required = true)]
        protected virtual void _(Events.CacheAttached<Contact.phone1> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault]
        [PXUIField(Required = true)]
        protected virtual void _(Events.CacheAttached<CRLead.lastName> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault]
        [PXUIField(Required = true)]
        protected virtual void _(Events.CacheAttached<CRLead.source> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault]
        [PXUIField(Required = true)]
        protected virtual void _(Events.CacheAttached<CRLead.campaignID> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault]
        [PXUIField(Required = true)]
        protected virtual void _(Events.CacheAttached<CRLead.description> e) { }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(Required = true)]
        [PXDefault]
        protected virtual void _(Events.CacheAttached<CRLead.classID> e) { }
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault]
        [PXUIField(Required = true)]
        protected virtual void _(Events.CacheAttached<CRLead.workgroupID> e) { }


        #endregion


        #region Events





        #endregion




    }
}