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
using System.IO;
using Newtonsoft.Json;
using PX.Commerce.Core.REST;
using RestSharp;
using RestSharp.Serializers;

namespace PX.Commerce.Shopify.API.REST
{
    public class RestJsonSerializer : IRestSerializer, IDeserializer, ISerializer
	{
        private readonly JsonSerializer _serializer;

        public RestJsonSerializer(JsonSerializer serializer)
        {
			AcceptedContentTypes = new string[]
			{
				"application/json",
				"text/json",
				"text/x-json"
			};
			_serializer = serializer;
        }

        public string Serialize(Parameter parameter)
        {
			return Serialize(parameter.Value);
		}

		public string Serialize(object obj)
		{
			using (var stringWriter = new StringWriter())
			{
				using (var jsonTextWriter = new JsonTextWriter(stringWriter))
				{
					jsonTextWriter.QuoteChar = '"';

					_serializer.Serialize(jsonTextWriter, obj);

					var result = stringWriter.ToString();
					return result;
				}
			}
		}

		public string DateFormat { get; set; }
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string ContentType { get; set; }
		public ISerializer Serializer { get { return this; } }
		public IDeserializer Deserializer { get { return this; } }
		public string[] AcceptedContentTypes { get; set; }
		public SupportsContentType SupportsContentType { get; set; }
		public DataFormat DataFormat { get; set; }

		public T Deserialize<T>(RestResponse response)
        {
			if (string.IsNullOrWhiteSpace(response.Content)) return default;

			JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new GetOnlyContractResolver() };
			try
			{
				return JsonConvert.DeserializeObject<T>(response.Content, settings);
			}
			catch (Exception ex)
			{
				throw new Exception($"{ex.Message}. Json data content: {response.Content}", ex);
			}
	
        }
	}
}
