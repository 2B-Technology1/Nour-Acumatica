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

using PX.Objects.FS;
using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;

public partial class Page_FS300400 : PX.Web.UI.PXPage
{
    public String applicationName;
    public String pageUrl;
    public String RefNbr;
    public String CustomerID;
    public String appointmentBridgeUrl;
    public String appointmentBodyTemplate;
    public String toolTipTemplateServiceOrder;
    public String toolTipTemplateAppointment;
    public String startDate;
    public String DefaultEmployee = "";
    public String ExternalEmployee;
    public String ExternalBranchID;
    public String ExternalBranchLocationID;
    public String SMEquipmentID;
    public String AppSource;

    protected void Page_Init(object sender, EventArgs e)
    {
        if (!Page.IsCallback)
        {
            var dict = SharedFunctions.GetCalendarMessages();
            this.ClientScript.RegisterClientScriptBlock(GetType(), "localeStrings", "var __localeStrings=" + Newtonsoft.Json.JsonConvert.SerializeObject(dict) + ";", true);
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        applicationName = Request.ApplicationPath.TrimEnd('/');
        pageUrl = SharedFunctions.GetWebMethodPath(Request.Path);

        DateTime? startDateBridge;
        var date = PXContext.GetBusinessDate();

        startDateBridge = (date != null) ? date : PXTimeZoneInfo.Now;

        // Filter By RefNbr
        RefNbr = Request.QueryString["RefNbr"];

        // External CustomerID
        CustomerID = Request.QueryString["CustomerID"];

        // Employee
        ExternalEmployee = String.IsNullOrEmpty(Request.QueryString["EmployeeID"]) == false ? Request.QueryString["EmployeeID"] : Request.QueryString["bAccountID"];

        // External SMEquipmentID
        SMEquipmentID = Request.QueryString["SMEquipmentID"];

        // Focus unassigned Appointment Tab
        AppSource = Request.QueryString["AppSource"];

        ExternalBranchID = Request.QueryString["branchID"];

        ExternalBranchLocationID = Request.QueryString["branchLocationID"];

        // Date
        try
        {
            if (!String.IsNullOrEmpty(Request.QueryString["Date"]))
            {
                startDateBridge = Convert.ToDateTime(Request.QueryString["Date"]);
            }
        }
        catch (Exception)
        {
        }

        var graphExternalControls = PXGraph.CreateInstance<ExternalControls>();
        var results = graphExternalControls.EmployeeSelected.Select();

        startDate = ((DateTime)startDateBridge).ToString("MM/dd/yyyy h:mm:ss tt", new CultureInfo("en-US"));

        PXResult<EPEmployee, Contact> result = (PXResult<EPEmployee, Contact>)results;

        EPEmployee epEmployeeRow = result;
        if (epEmployeeRow != null)
        {
            DefaultEmployee = epEmployeeRow.BAccountID.ToString();
        }

        if (string.IsNullOrEmpty(ExternalEmployee) && epEmployeeRow != null)
        {
        ExternalEmployee = DefaultEmployee;
        }

        // Load Appointment's Body to be used in index.aspx
        StreamReader streamReader = new StreamReader(Server.MapPath("../../Shared/templates/EventTemplate.html"));
        appointmentBodyTemplate = streamReader.ReadToEnd();
        streamReader.Close();

        // Load Service Order's ToolTip to be used in index.aspx
        streamReader = new StreamReader(Server.MapPath("../../Shared/templates/TooltipServiceOrder.html"));
        toolTipTemplateServiceOrder = streamReader.ReadToEnd();
        streamReader.Close();

        // Load Appointment's ToolTip to be used in index.aspx
        streamReader = new StreamReader(Server.MapPath("../../Shared/templates/TooltipAppointment.html"));
        toolTipTemplateAppointment = streamReader.ReadToEnd();
        streamReader.Close();
    }

}