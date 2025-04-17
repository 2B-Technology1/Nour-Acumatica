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
using Autofac;
using CommonServiceLocator;
using PX.Api.ContractBased.UI.DAC;
using PX.Commerce.Core;
using PX.Data;
using PX.Data.Maintenance.GI;
using PX.Metadata;
using PX.Objects.CR;

namespace PX.Commerce.Objects
{
	#region ConnectorService
	public class ConnectorService : IConnectorService
	{
		public class Definition : IPrefetchable
		{
			public const string SLOT = "BCConnectorsDef";

			public List<Tuple<String, String, Boolean>> AllEntities = new List<Tuple<String, String, Boolean>>();
			public Dictionary<String, ConnectorInfo> Connectors = new Dictionary<String, ConnectorInfo>();
			public Dictionary<String, List<EntityInfo>> ConnectorEntities = new Dictionary<String, List<EntityInfo>>();
			public Dictionary<String, List<BindingInfo>> ConnectorStores = new Dictionary<String, List<BindingInfo>>();
			
			public void Prefetch()
			{
				AllEntities = new List<Tuple<String, String, Boolean>>();
				Connectors = new Dictionary<String, ConnectorInfo>();
				ConnectorEntities = new Dictionary<String, List<EntityInfo>>();
				ConnectorStores = new Dictionary<String, List<BindingInfo>>();

				IReadOnlyDictionary<string, IConnectorFactory> factories = ServiceLocator.Current.GetInstance<IReadOnlyDictionary<string, IConnectorFactory>>();
				foreach (KeyValuePair<string, IConnectorFactory> pair in factories)
				{
					Connectors[pair.Key] = new ConnectorInfo() { ConnectorType = pair.Key, ConnectorName = pair.Value.Description, IsActive = (pair.Value?.Enabled ?? false) };

					List<EntityInfo> entities = ConnectorEntities[pair.Key] = pair.Value.GetProcessors();

					List<BindingInfo> stores = ConnectorStores[pair.Key] = new List<BindingInfo>();
					foreach (PXDataRecord rec in PXDatabase.SelectMulti<BCBinding>(
						new PXDataField<BCBinding.bindingID>(),
						new PXDataField<BCBinding.bindingName>(),
						new PXDataField<BCBinding.isActive>(),
						new PXDataField<BCBinding.isDefault>(),
						new PXDataFieldValue<BCBinding.connectorType>(pair.Key)
						))
					{
						stores.Add(new BindingInfo() 
						{ 
							BindingID = rec.GetInt32(0).Value, 
							BindingName = rec.GetString(1), 
							IsActive = (rec.GetBoolean(2) ?? false) && (pair.Value?.Enabled ?? false), 
							IsDefault = (rec.GetBoolean(3) ?? false) });
					}
					foreach (PXDataRecord rec in PXDatabase.SelectMulti<BCEntity>(
						new PXDataField<BCEntity.bindingID>(),
						new PXDataField<BCEntity.entityType>(),
						new PXDataField<BCEntity.direction>(),
						new PXDataField<BCEntity.primarySystem>(),
						new PXDataField<BCEntity.isActive>(),
						new PXDataFieldValue<BCEntity.connectorType>(pair.Key)
						))
					{
						Int32? bindingID = rec.GetInt32(0);
						String entityType = rec.GetString(1);
						String direction = rec.GetString(2);
						String primary = rec.GetString(3);
						Boolean? isActive = rec.GetBoolean(4);

						EntityInfo entity = entities.FirstOrDefault(s => s.EntityType == entityType);
						if (entity != null)
						{
							if (direction != null) entity.ActualDirection = BCSyncDirectionAttribute.Convert(direction);
							if (primary != null) entity.ActualPrimarySystem = BCSyncSystemAttribute.Convert(primary);
						}

						BindingInfo binding = stores.FirstOrDefault(s => s.BindingID == bindingID);
						if (binding != null && (pair.Value?.Enabled ?? false))
						{
							if (isActive == true)
								binding.ActiveEntities.Add(entityType);

							binding.AllCreatedEntities.Add(entityType);
						}
					}

					foreach (EntityInfo entity in entities)
					{
						Tuple<String, String, Boolean> found = AllEntities.FirstOrDefault(s => s.Item1 == entity.EntityType);
						if (found == null) AllEntities.Add(Tuple.Create(entity.EntityType, entity.EntityName, true));

						if (entity.DetailTypes != null && entity.DetailTypes.Length > 0)
						{
							for (int i = 0; i < entity.DetailTypes.Length; i++)
							{
								EntityDetailInfo detail = entity.DetailTypes[i];

								found = AllEntities.FirstOrDefault(s => s.Item1 == detail.EntityType);
								if (found == null) AllEntities.Add(Tuple.Create(detail.EntityType, detail.EntityName, false));
							}
						}
					}
				}
			}
		}
		public class SchemaDefiniton : IPrefetchable<SchemaDefiniton.SchemaRequest>
		{
			public const string SLOT = "BCConnectorsDef";
			public const string StockItemScreenID = "IN202500";
			public const string NonStockItemScreenID = "IN202000";

			public class SchemaRequest
			{
				public String ConnectorType;
				public Int32? BindingID;
				public EntityInfo Entity;
				public IConnector Connector;
			}
			public SchemaInfo ConnectorSchema;

			public void Prefetch(SchemaRequest request)
			{
				ConnectorSchema = new SchemaInfo();
				ConnectorSchema.EntityType = request.Entity.EntityType;
                ConnectorSchema.FormFields = request.Connector.GetExternalFields(request.BindingID, request.Entity.EntityType);
				ConnectorSchema.CustomFields = GetCustomFields(request);
                if ( request.Entity.AcumaticaPrimaryType != null)
                {
                    ConnectorSchema.Attributes = CRAttribute.EntityAttributes(request.Entity.AcumaticaPrimaryType.FullName).Select(x => new Tuple<string, string>(x.Description, x.Description)).ToList();
                }

			}

			//TODO Need review and improve this method
			private static List<CustomFieldInfo> GetCustomFields(SchemaRequest request)
			{
				Type cbApiType = request.Entity.LocalTypes.FirstOrDefault();
				if (cbApiType == null) return new List<CustomFieldInfo>();

				string screenID;
				string sEndPoint = String.Empty;
				string sEndPointVersion = String.Empty;
				using (PXDataRecord rec = PXDatabase.SelectSingle<BCBinding>(
											new PXDataField<BCBinding.webServicesEndpoint>(),
											new PXDataField<BCBinding.webServiceVersion>(),
											new PXDataFieldValue<BCBinding.connectorType>(request.ConnectorType),
											new PXDataFieldValue<BCBinding.bindingID>(request.BindingID)))
				{
					sEndPoint = rec?.GetString(0);
					sEndPointVersion = rec?.GetString(1);
				}

				if (!string.IsNullOrEmpty(request.Entity.GIScreenID))
				{
					//Get the screen from Processor if it's a GI object.
					screenID = request.Entity.GIScreenID;
					cbApiType = request.Entity.GIResult;
				}
				else
				{
					screenID = GetScreenIDForEndPointEntity(sEndPoint, sEndPointVersion, cbApiType.Name);
				}

				ICBAPIServiceFactory factory = ServiceLocator.Current.GetInstance<ICBAPIServiceFactory>();
				List<CustomFieldInfo> customFieldsDict = null;
				using(Core.Model.ICBAPIService service = factory.GetService(sEndPoint, sEndPointVersion, String.Empty, 0, null))
				{
					customFieldsDict = service.GetCustomFieldsSchema(cbApiType, screenID, request.Entity);
				}
				
				if (request.Entity.PrimaryGraph != null && customFieldsDict?.Count > 0)
				{
					var fieldsList = customFieldsDict.Select(x => new { x.Container, x.Field.ViewName, x.Field.FieldName }).Distinct().ToList();
					
					PXSiteMap.ScreenInfo screen = screenID != null ? Api.ScreenUtils.ScreenInfo.TryGet(screenID) : null;
					if (screen != null)
					{
						IEnumerable<KeyValuePair<string, Data.Description.PXViewDescription>> fieldContainers = screen.Containers.Where(x => fieldsList.Any(f => f.ViewName == x.Key || x.Key.StartsWith($"{f.ViewName}:"))).Distinct();
						fieldsList.ForEach(item =>
						{
							List<Data.Description.PXViewDescription> viewDescription = fieldContainers.Where(x => x.Key == item.ViewName || x.Key.StartsWith($"{item.ViewName}:")).Select(f => f.Value).ToList();
							Data.Description.FieldInfo fieldInfo = viewDescription?.SelectMany(x => x.Fields).FirstOrDefault(f => f.FieldName == item.FieldName);
							if (fieldInfo != null)
							{
								customFieldsDict.Where(x => x.Container == item.Container && x.Field.FieldName == item.FieldName && x.Field.ViewName == item.ViewName).All(x =>
								{
									x.DisplayName = fieldInfo.DisplayName;
									x.FieldType = fieldInfo.FieldType;
									return true;
								});
							}
						});
					}
				}
				return customFieldsDict;
			}
		}
		
		/// <summary>
		/// Looks for an entitydescription record  for in an endpoint;version and entity name 
		/// the entity can be describe in the endpoint itself or in the parent endpoing recursively.
		/// </summary>
		/// <param name="endPoint"></param>
		/// <param name="version"></param>
		/// <param name="entityName"></param>
		/// <returns></returns>
		private static string GetScreenIDForEndPointEntity(string endPoint, string version, string entityName)
		{
			string screenId = null;
			using (PXDataRecord rec = PXDatabase.SelectSingle<EntityDescription>(
											new PXDataField<EntityDescription.screenID>(),
											new PXDataFieldValue<EntityDescription.interfaceName>(endPoint),
											new PXDataFieldValue<EntityDescription.gateVersion>(version),
											new PXDataFieldValue<EntityDescription.objectName>(entityName)))
			{
				screenId = rec?.GetString(0);
			}

			if (!string.IsNullOrEmpty(screenId)) { return screenId; }

			//Look for the upper endpoint
			bool hasParent = false;
			using (PXDataRecord rec = PXDatabase.SelectSingle<EntityEndpoint>(
											new PXDataField<EntityEndpoint.extendsName>(),
											new PXDataField<EntityEndpoint.extendsVersion>(),
											new PXDataFieldValue<EntityEndpoint.interfaceName>(endPoint),
											new PXDataFieldValue<EntityEndpoint.gateVersion>(version)))
			{
				if (rec != null)
				{
					endPoint = rec.GetString(0);
					version = rec.GetString(1);
					hasParent = true;
				}
			}

			if (hasParent)
				screenId = GetScreenIDForEndPointEntity(endPoint, version, entityName);

			return screenId;
		}
		protected Definition GetDefinition()
		{
			return PXDatabase.GetSlot<Definition>(Definition.SLOT, typeof(BCBinding), typeof(BCEntity));
		}
		protected SchemaDefiniton GetSchemaDefinition(SchemaDefiniton.SchemaRequest request)
		{
			return PXDatabase.GetSlot<SchemaDefiniton, SchemaDefiniton.SchemaRequest>(Definition.SLOT + request.Entity.EntityType,
				request, typeof(BCBinding), typeof(BCEntity), typeof(PX.Objects.CS.CSAttributeGroup), typeof(GIResult));
		}

		public List<ConnectorInfo> GetConnectors()
		{
			Definition def = GetDefinition();

			return def?.Connectors?.Values?.ToList();
		}
		public IConnector GetConnector(String code)
		{
			if (String.IsNullOrEmpty(code)) return null;

			if (ServiceLocator.IsLocationProviderSet)
			{
				IReadOnlyDictionary<string, IConnectorFactory> factories = ServiceLocator.Current.GetInstance<IReadOnlyDictionary<string, IConnectorFactory>>();
				if (factories.ContainsKey(code))
				{
					return factories[code].GetConnector();
				}
			}
			return null;
		}

		public List<EntityInfo> GetConnectorEntites(String code)
		{
			if (String.IsNullOrEmpty(code)) return null;

			Definition def = GetDefinition();

			return def.ConnectorEntities.ContainsKey(code) ? def.ConnectorEntities[code] : null;
		}

		/// <summary>
		/// Returns a list of entities of connector whose dependent feature is enabled.
		/// </summary>
		public List<EntityInfo> GetConnectorEntitiesWithFeaturesEnabled(String code)
		{
			var connectorEntities = GetConnectorEntites(code);
			if (connectorEntities == null) return null;

			List<Tuple<string, EntityInfo>> disabledEntities = GetEntitesDisabledByFeatures();

			return connectorEntities.Where(x => !disabledEntities.Any(y => y.Item1 == code && y.Item2.EntityType == x.EntityType)).ToList();
		}

		public EntityInfo GetConnectorEntity(String connectorCode, String entityType)
		{
			List<EntityInfo> entities = GetConnectorEntites(connectorCode);
			return entities == null ? null : entities.Where(x => x.EntityType == entityType).FirstOrDefault();
		}
		
		public List<Tuple<String, String, Boolean>> GetAllEntities()
		{
			Definition def = GetDefinition();

			return def.AllEntities;
		}

		public List<BindingInfo> GetConnectorBindings(String code)
		{
			if (String.IsNullOrEmpty(code)) return null;

			Definition def = GetDefinition();

			return def.ConnectorStores.ContainsKey(code) ? def.ConnectorStores[code] : null;
		}
		public BindingInfo GetConnectorBinding(String code, Int32? bindingID)
		{
			List<BindingInfo> bindings = GetConnectorBindings(code);
			return bindings == null ? null : bindings.Where(x => x.BindingID == bindingID).FirstOrDefault();
		}

		public SchemaInfo GetConnectorSchema(String code, Int32? binding, String entityType)
		{
			if (String.IsNullOrEmpty(code)) return null;

			EntityInfo entity = GetConnectorEntity(code, entityType);
			IConnector connector = GetConnector(code);
			if (entity != null && connector != null)
			{
				SchemaDefiniton def = GetSchemaDefinition(new SchemaDefiniton.SchemaRequest() { Connector = connector, ConnectorType = code, BindingID = binding, Entity = entity });

				return def.ConnectorSchema;
			}
			return null;
		}

		public void CleanEntitySLOTCache(String connectorCode, Int32? bindingID, String entityType)
		{
			if (String.IsNullOrEmpty(connectorCode)) return;

			EntityInfo entity = GetConnectorEntity(connectorCode, entityType);
			PXDatabase.ResetSlot<SchemaDefiniton>(Definition.SLOT + entity.EntityType,
				typeof(BCBinding), typeof(BCEntity), typeof(PX.Objects.CS.CSAttributeGroup), typeof(GIResult));
		}

		/// <summary>
		/// Returns a list of entities, alongside with connector code, among all connectors whose dependent feature is disabled.
		/// </summary>
		public List<Tuple<string, EntityInfo>> GetEntitesDisabledByFeatures()
		{
			Definition def = GetDefinition();
			List<Tuple<string, EntityInfo>> disabledEntities = new List<Tuple<string, EntityInfo>>();

			foreach (var connectorEntity in def.ConnectorEntities)
			{
				string connectorCode = connectorEntity.Key;

				foreach (var entity in connectorEntity.Value)
				{
					if (BCExtensions.IsEntityFeatureDisabled(entity))
						disabledEntities.Add(Tuple.Create(connectorCode, entity));
				}
			}

			return disabledEntities;
		}
	}
	#endregion

	#region ConnectorServiceRegistration
	internal class ConnectorServiceRegistration : Autofac.Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder
				.RegisterType<ConnectorService>()
				.As<IConnectorService>()
				.SingleInstance();
		}
	}
	#endregion
}
