using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;
using PX.SM;
using PX.Web.UI;
using System.Collections.Generic;
using CommonServiceLocator;

public partial class Menu : PX.Web.UI.PXPage
{
	#region Page event handlers

	protected override void OnInit(EventArgs e)
	{
		if (!this.IsCallback)
		{
			// set company logo
			PXResult<Branch, UploadFile> res =
				(PXResult<Branch, UploadFile>)PXSelectJoin<Branch,
					InnerJoin<UploadFile, On<Branch.logoName, Equal<UploadFile.name>>>,
					Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.
					Select(new PXGraph(), ServiceLocator.Current.GetInstance<ICurrentUserInformationProvider>().GetBranchCD());
			if (res != null)
			{
				PX.SM.UploadFile file = (PX.SM.UploadFile)res;
				if (file != null)
					this.logoCell.Style[HtmlTextWriterStyle.BackgroundImage] = ControlHelper.GetAttachedFileUrl(this, file.FileID.ToString());
			}
		}
		base.OnInit(e);
	}

	protected override void OnPreRender(EventArgs e)
	{
		if (ControlHelper.IsRtlCulture())
			this.Page.Form.Style[HtmlTextWriterStyle.Direction] = "rtl";
		base.OnPreRender(e);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		var source = new List<PXSiteMapNode>();
		var wiki = PXSiteMap.WikiProvider;
		var wikiRoot = wiki.RootNode;

		if (this.IsCallback)
			this.EnableViewStateMac = false;

		this.InitSearchBox();
		foreach (var node in PXSiteMap.RootNode.ChildNodes)
		{
			PX.Data.PXSiteMapNode swNode = node;
			if (swNode == null)
			    source.Add(node);
		}

		navPanel.DataBindings.TextField = "Title";
		navPanel.DataBindings.ImageUrlField = "Description";
		navPanel.DataBindings.ContentUrlField = "Url";
		navPanel.DataSource = source;
		navPanel.DataBind();
		bindComplete = false;
	}

	protected void navPanel_OnDataBound(object sender, EventArgs e)
	{
		bindComplete = true;
	}
	#endregion

	#region Private methods

	private bool SyncAvailable
	{
		get { return btnSyncMenu.Visible; }
		set { btnSyncMenu.Visible = value; }
	}

	private int CallbackIndex
	{
		get
		{
			int callbackIndex = 0;
			string spArgs = this.navPanel.ClientID.ToString() + "$sp";
			if (this.IsCallback &&
				 this.Request.Params["__CALLBACKID"].StartsWith(spArgs))
			{
				int.TryParse(this.Request.Params["__CALLBACKID"].Substring(spArgs.Length), out callbackIndex);
			}
			if (this.IsCallback &&
				this.Request.Params["__CALLBACKID"].EndsWith("$treeHlp"))
			{
				callbackIndex = this.navPanel.Bars.Count - 1;
			}
			return callbackIndex;
		}
	}

	private void InitSearchBox()
	{
		PXSearchBox box = this.srch;
		box.SearchNavigateUrl = this.ResolveUrl/**/("~/Search/Search.aspx") + "?query=";
		box.Text = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.SearchBox.TypeYourQueryHere);
		box.ToolTip = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.SearchBox.SearchSystem);
		box.AddNewVisible = false;
	}
	#endregion

	#region Fields

	bool bindComplete;
	#endregion

}
