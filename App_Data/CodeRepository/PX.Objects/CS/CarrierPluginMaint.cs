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
using System.Text;
using PX.Data;
using System.Web.Compilation;
using PX.SM;
using PX.Data.Update;
using PX.Objects.IN;
using System.Collections;
using PX.CarrierService;
using PX.DbServices;
using PX.Archiver;

namespace PX.Objects.CS
{
	public class CarrierPluginMaint : PXGraph<CarrierPluginMaint, CarrierPlugin>
	{
		public PXSelect<CarrierPlugin> Plugin;
		public PXSelect<CarrierPluginDetail> Details;
		public PXSelect<CarrierPluginDetail, Where<CarrierPluginDetail.carrierPluginID, Equal<Current<CarrierPlugin.carrierPluginID>>>> SelectDetails;
		protected IEnumerable details()
		{
			ImportSettings();

			return SelectDetails.Select();
		}


		public PXSelect<CarrierPluginCustomer, Where<CarrierPluginCustomer.carrierPluginID, Equal<Current<CarrierPlugin.carrierPluginID>>>> CustomerAccounts;


		
		public virtual void ImportSettings()
		{
			if (Plugin.Current != null)
			{
				CarrierResult<ICarrierService> serviceResult = CreateCarrierService(this, Plugin.Current);
				if (serviceResult.IsSuccess)
				{
					IList<ICarrierDetail> details = serviceResult.Result.ExportSettings();
					InsertDetails(details);
				}
			}
		}

		public PXAction<CarrierPlugin> certify;
		[PXUIField(DisplayName = "Prepare Certification Files", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual void Certify()
		{
			if (Plugin.Current != null)
			{
				Save.Press();

				CarrierPlugin currentRow = Plugin.Current;
				PXLongOperation.StartOperation(this, delegate()
				{
					CarrierPluginMaint docgraph = PXGraph.CreateInstance<CarrierPluginMaint>();
					docgraph.PrepareCertificationData(currentRow);
				});
			}

		}

		public PXAction<CarrierPlugin> test;
		[PXUIField(DisplayName = "Test connection", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton(IsLockedOnToolbar = true)]
		public virtual void Test()
		{
			if (Plugin.Current != null)
			{
				Save.Press();

				CarrierResult<ICarrierService> serviceResult = CreateCarrierService(this, Plugin.Current, true);
				if (serviceResult.IsSuccess)
				{
					CarrierResult<string> result = serviceResult.Result.Test();
					if (result.IsSuccess)
					{
						Plugin.Ask(Plugin.Current, Messages.ConnectionCarrierAskSuccessHeader, Messages.ConnectionCarrierAskSuccess, MessageButtons.OK, MessageIcon.Information);
					}
					else
					{
						StringBuilder errorMessages = new StringBuilder();

						foreach (Message message in result.Messages)
						{
							errorMessages.AppendLine(message.Description);
						}

						if (errorMessages.Length > 0)
						{
							throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.TestFailed, errorMessages.ToString()));
						}

					}
				}
			}

		}

		private void PrepareCertificationData(CarrierPlugin cp)
		{
			CarrierResult<ICarrierService> serviceResult = CreateCarrierService(this, cp, true);
			if (serviceResult.IsSuccess)
			{
				UploadFileMaintenance upload = PXGraph.CreateInstance<UploadFileMaintenance>();
				CarrierResult<IList<CarrierCertificationData>> result = serviceResult.Result.GetCertificationData();

				if (result != null)
				{
					StringBuilder sb = new StringBuilder();
					foreach (Message message in result.Messages)
					{
						sb.AppendFormat("{0}:{1} ", message.Code, message.Description);
					}

					if (result.IsSuccess)
					{
						CarrierPlugin copy = (CarrierPlugin) Plugin.Cache.CreateCopy(cp);

						using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
						{
							using (ZipArchiveWrapper zip = new ZipArchiveWrapper(ms))
							{
								foreach (CarrierCertificationData d in result.Result)
								{
									zip[string.Format("{0}.{1}", d.Description, d.Format)] = d.File;
								}
							}

							FileInfo file = new FileInfo("CertificationData.zip", null, ms.ToArray());
							upload.SaveFile(file, FileExistsAction.CreateVersion);
							PXNoteAttribute.SetFileNotes(Plugin.Cache, copy, file.UID.Value);
						}

						Plugin.Update(copy);

						this.Save.Press();
					}
					else
					{
						throw new PXException(SO.Messages.CarrierServiceError, sb.ToString());
					}
				}
			}
		}
		
		public virtual void InsertDetails(IList<ICarrierDetail> list)
		{
			Dictionary<string, CarrierPluginDetail> existingRecords = new Dictionary<string, CarrierPluginDetail>();
			foreach (CarrierPluginDetail detail in SelectDetails.Select())
			{
				existingRecords.Add(detail.DetailID.ToUpper(), detail);
			}

			foreach (ICarrierDetail item in list)
			{
				if (!existingRecords.ContainsKey(item.DetailID.ToUpper()))
				{
					CarrierPluginDetail row = (CarrierPluginDetail)SelectDetails.Insert(new CarrierPluginDetail { DetailID = item.DetailID });
					row.Descr = item.Descr;
					row.Value = item.Value;
					row.ControlType = item.ControlType;
					row.SetComboValues(item.GetComboValues());
				}
				else
				{
					CarrierPluginDetail cd = existingRecords[item.DetailID];
					CarrierPluginDetail copy = PXCache<CarrierPluginDetail>.CreateCopy(cd);
					
					if (!string.IsNullOrEmpty(item.Descr))
						copy.Descr = item.Descr;
					copy.ControlType = item.ControlType;
					copy.SetComboValues(item.GetComboValues());
					
					if ( cd.Descr != copy.Descr || cd.ControlType != copy.ControlType || cd.ComboValues != copy.ComboValues )
					{
						SelectDetails.Update(copy);

						//Set Cache's IsDirty property to false to avoid enabling save button on loading the details for the carrier plugin
						SelectDetails.Cache.IsDirty = false;
					}
				}
			}

		}

		public static CarrierResult<ICarrierService> CreateCarrierService(PXGraph graph, CarrierPlugin plugin, bool throwException = false)
		{
			CarrierResult<ICarrierService> result;
			ICarrierService service = null;
			string error = string.Empty;

			if (!string.IsNullOrEmpty(plugin.PluginTypeName))
			{
				Type carrierType = null;

				try
				{
					carrierType = PXBuildManager.GetType(plugin.PluginTypeName, true);
					service = (ICarrierService)Activator.CreateInstance(carrierType);

					PXSelectBase<CarrierPluginDetail> select = new PXSelect<CarrierPluginDetail, Where<CarrierPluginDetail.carrierPluginID, Equal<Required<CarrierPluginDetail.carrierPluginID>>>>(graph);
					PXResultset<CarrierPluginDetail> resultset = select.Select(plugin.CarrierPluginID);
					IList<ICarrierDetail> list = new List<ICarrierDetail>(resultset.Count);

					foreach (CarrierPluginDetail item in resultset)
					{
						list.Add(item);
					}

					service.LoadSettings(list);
				}
				catch (Exception ex)
				{
					error = carrierType != null ? string.Format(Messages.FailedToCreateCarrierPlugin, ex.Message) :
						string.Format(Messages.FailedToCreateCarrierService, plugin.CarrierPluginID);

					if (throwException)
					{
						throw new PXException(error);
					}
				}
			}

			result = new CarrierResult<ICarrierService>(service != null, service, new Message(string.Empty, error));
			return result;
		}

		public static IList<string> GetCarrierPluginAttributes(PXGraph graph, string carrierPluginID)
		{
			CarrierPlugin plugin = CarrierPlugin.PK.Find(graph, carrierPluginID);

			if (plugin == null)
				throw new PXException(Messages.FailedToFindCarrierPlugin, carrierPluginID);

			ICarrierService service = null;
			if (!string.IsNullOrEmpty(plugin.PluginTypeName))
			{
				Type carrierType = PXBuildManager.GetType(plugin.PluginTypeName, false);
				service = carrierType != null ? (ICarrierService)Activator.CreateInstance(carrierType) : null;
			}

			return service == null ? new List<String>() : service.Attributes;
		}

		public static CarrierResult<ICarrierService> CreateCarrierService(PXGraph graph, string carrierPluginID, bool throwException = false)
		{
			CarrierPlugin plugin = CarrierPlugin.PK.Find(graph, carrierPluginID);

			if (plugin == null)
				throw new PXException(Messages.FailedToFindCarrierPlugin, carrierPluginID);

			CarrierResult<ICarrierService> serviceResult = CreateCarrierService(graph, plugin, throwException);
			return serviceResult;
		}

		public static bool IsValidType(string type) => PXBuildManager.GetType(type, false) != null;

		protected virtual void CarrierPlugin_PluginTypeName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CarrierPlugin row = e.Row as CarrierPlugin;
			if (row == null) return;

			foreach (CarrierPluginDetail detail in SelectDetails.Select())
			{
				SelectDetails.Delete(detail);
			}
		}

		protected virtual void CarrierPluginDetail_Value_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CarrierPluginDetail row = e.Row as CarrierPluginDetail;
			if (row != null)
			{
				string fieldName = typeof(CarrierPluginDetail.value).Name;

				switch (row.ControlType)
				{
					case CarrierPluginDetail.Combo:
						List<string> labels = new List<string>();
						List<string> values = new List<string>();
						foreach (KeyValuePair<string, string> kv in row.GetComboValues())
						{
							values.Add(kv.Key);
							labels.Add(kv.Value);
						}
						e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CarrierDetail.ValueFieldLength, null, fieldName, false, 1, null,
																values.ToArray(), labels.ToArray(), true, null);
						break;
					case CarrierPluginDetail.CheckBox:
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(Boolean), false, null, -1, null, null, null, fieldName,
								null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
						break;
					case CarrierPluginDetail.Password:
						if (e.ReturnState != null)
						{
							string strValue = e.ReturnState.ToString();
							string encripted = new string('*', strValue.Length);

							e.ReturnState = PXFieldState.CreateInstance(encripted, typeof(string), false, null, -1, null, null, null, fieldName,
									null, null, null, PXErrorLevel.Undefined, null, null, null, PXUIVisibility.Undefined, null, null, null);
						}
						break;
					default:
						break;
				}
			}
		}

		protected virtual void CarrierPlugin_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			CarrierPlugin row = e.Row as CarrierPlugin;
			if (row == null) return;

			Carrier shipVia = PXSelect<Carrier, Where<Carrier.carrierPluginID, Equal<Current<CarrierPlugin.carrierPluginID>>>>.SelectWindowed(this, 0, 1);
			if (shipVia != null)
			{
				throw new PXException(Messages.ShipViaFK);
			}

		}

		protected virtual void CarrierPlugin_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CarrierPlugin row = e.Row as CarrierPlugin;
			if (row == null) return;

			CarrierResult<ICarrierService> serviceResult = CreateCarrierService(this, row);
			//set the warning to null by default and attach the appropriate warnings later
			PXUIFieldAttribute.SetWarning<CarrierPlugin.pluginTypeName>(sender, row, null);

			if (serviceResult.IsSuccess)
			{
				certify.SetVisible(serviceResult.Result.Attributes.Contains("CERTIFICATE"));
			}
			else
			{
				string warning = string.Format(serviceResult.Messages?.FirstOrDefault()?.Description, row?.CarrierPluginID);
				PXUIFieldAttribute.SetWarning<CarrierPlugin.pluginTypeName>(sender, row, warning);
			}
		}

		protected virtual void _(Events.FieldUpdated<CarrierPlugin, CarrierPlugin.unitType> args)
		{
			Plugin.Cache.SetValueExt<CarrierPlugin.kilogramUOM>(args.Row, null);
			Plugin.Cache.SetValueExt<CarrierPlugin.poundUOM>(args.Row, null);
			Plugin.Cache.SetValueExt<CarrierPlugin.centimeterUOM>(args.Row, null);
			Plugin.Cache.SetValueExt<CarrierPlugin.inchUOM>(args.Row, null);
		}
	}
}
