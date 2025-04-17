using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.SM;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.Repositories;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CR.Extensions;
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using PX.Data.Descriptor;
using CashAccountAttribute = PX.Objects.GL.CashAccountAttribute;
using PX.Objects.GL.Helpers;
using PX.Objects.TX;
using PX.Objects.IN;
using PX.Objects.CR.Extensions.Relational;
using PX.Objects.CR.Extensions.CRCreateActions;
using PX.Objects.GDPR;
using PX.Objects.GraphExtensions.ExtendBAccount;
using PX.Data.ReferentialIntegrity.Attributes;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects;
using PX.Objects.AR;
using Nour20231012VSolveUSDNew;
using MyMaintaince;
using Nour2024.Helpers;
using Nour2024.Models;
using Newtonsoft.Json;
using PX.Objects.CR.MassProcess;

namespace PX.Objects.AR
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public class CustomerMaint_Extension : PXGraphExtension<PX.Objects.AR.CustomerMaint>
  {
        public override void Initialize()
        {
            PXSiteMap.CurrentNode.Title = "Reserve Form";
        }


        public PXAction<Customer> CreateReserveForm;
        [PXButton]
        [PXUIField(DisplayName = "Create ReserveForm", Enabled = true)]
        protected virtual IEnumerable createReserveForm(PXAdapter adapter)
        {

            ReserveFormMaint reserveGraph = PXGraph.CreateInstance<ReserveFormMaint>();
            reserveGraph.reserveForm.Insert(new ReserveForm());
            throw new PXRedirectRequiredException(reserveGraph, true, "Functional Location");
            //return adapter.Get();
        }



        public PXAction<Customer> SendSMS;
        [PXButton(Connotation = Data.WorkflowAPI.ActionConnotation.Warning)]
        [PXUIField(DisplayName = "SendSMS", Enabled = true)]
        protected virtual IEnumerable sendSMS(PXAdapter adapter)
        {
            string acctCd = Base?.BAccount?.Current?.AcctCD;
            int? bAcountID = Base?.BAccount?.Current?.BAccountID;

            string message = $"عميلنا العزيز يشرّفنا زيارتكم لنا بشركة نور الدين الشرف - كود زيارة رقم {acctCd} لمزيد من الاستفسارات اتصل بنا على 19943";

            Customer customer = SelectFrom<Customer>.Where<Customer.acctCD.IsEqual<@P.AsString>>.View.Select(Base, acctCd);
            SOOrderEntry soOrderGraph = PXGraph.CreateInstance<SOOrderEntry>();
            Contact contact = SelectFrom<Contact>.Where<Contact.bAccountID.IsEqual<@P.AsInt>>.View.Select(soOrderGraph, bAcountID);

            string phone = contact?.Phone1;
            if (phone != null && acctCd != "<NEW>")
            {
                string response = SmsSender.SendMessage(message, phone);
                salssmsrespons responseRoot = JsonConvert.DeserializeObject<salssmsrespons>(response);

                if (responseRoot != null)
                {
                    if (responseRoot.message == "success")
                    {
                        // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                        throw new PXException("Message sent successfully");
                    }
                }
            }
            else
            {
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                throw new PXException("Phone Number is missing\nplease make sure you inserted the phone number and save");
            }
            return adapter.Get();
        }

        #region Event Handlers


        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
        [PhoneValidation()]
        [PXMassMergableField]
        [PXDeduplicationSearchField]
        [PXPersonalDataField]
        [PXPhone]
        [PXContactInfoField]
        [PXDefault]
        protected virtual void Contact_Phone1_CacheAttached(PXCache cache)
        {

        }


        #endregion
    }
}