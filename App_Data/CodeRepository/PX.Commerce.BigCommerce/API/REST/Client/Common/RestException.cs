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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using RestSharp;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class RestException : Exception
	{
		public string ResponceMessage;
		public string ResponceStatusCode;
		public string ResponceContent;
		public RestResponse Response;
		protected readonly List<RestError> _errors;

		public string ErrorMessage => base.Message;

		public RestException(RestResponse response)
		: base(GetErrorMessage(response))
		{
			Response = response;
			ResponceMessage = response.ErrorMessage;
			ResponceStatusCode = response.StatusCode.ToString();
			ResponceContent = response.Content;

			_errors = GetErrorData(response) ?? new List<RestError>();
		}

		public override string ToString()
		{
			return GetErrorMessage(Response);
		}

		public static String GetErrorMessage(RestResponse response)
		{
			StringBuilder sb = new StringBuilder();

			//Returned Errors
			bool errorsPersist = false;
			List<RestError> restErrors = GetErrorData(response);
			if (restErrors != null)
			{
				for (int i = 0; i < restErrors.Count; i++)
				{
					if (!String.IsNullOrEmpty(restErrors[i].ToString()))
					{
						errorsPersist = true;
						sb.AppendFormat("Error: {1};", i, CustomMessage(response, restErrors[i]));
						sb.AppendLine();
					}
				}
			}
			//if no errors parsed, display rough Content
			if (!errorsPersist)
			{
				//Content
				if (!String.IsNullOrEmpty(response.Content))
				{
					sb.AppendLine($"Content:  {response.Content}");
					sb.AppendLine();
				}


			}

			sb.Append($"Status: {response.StatusCode}");

			return sb.ToString();
		}

		public static List<RestError> GetErrorData(RestResponse response)
		{
			if (response == null ||
				response.StatusCode == HttpStatusCode.OK ||
				response.StatusCode == HttpStatusCode.Created ||
				response.StatusCode == HttpStatusCode.Accepted ||
				response.StatusCode == HttpStatusCode.NoContent)
			{
				return null;
			}

			List<RestError> errorList = new List<RestError>();

			String content = response.Content;
			RestError1[] result1 = TryDeserialize<RestError1[]>(content);
			if (result1 != null)
			{
				foreach (RestError error in result1)
				{
					errorList.Add(error);
				}
			}
			else
			{
				RestError2 result2 = TryDeserialize<RestError2>(content);
				if (result2 != null)
				{
					errorList.Add(result2);
				}
				else
				{
					RestError3 result3 = TryDeserialize<RestError3>(content);
					if (result3 != null)
					{
						errorList.Add(result3);
					}
				}
			}

			if (errorList.Count <= 0)
			{
				errorList.Add(new RestError1
				{
					Status = (int)response.StatusCode,
					Message = response.Content
				});
			}

			return errorList;
		}

		public static string CustomMessage(RestResponse response, RestError error)
		{
			RestError2 Error = RestException.TryDeserialize<RestError2>(response.Content);
			if (Error?.Errors != null)
				if (Error?.Errors.ContainsKey("custom_url") == true)
				{
					var request = response.Request.Parameters.First(x => x.Type == ParameterType.RequestBody).Value;
					if (request != null)
					{
						try
						{
							Type myType = request.GetType();
							IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
							PropertyInfo prop = props.FirstOrDefault(i => i.Name == "CustomUrl");
							if (prop != null)
							{
								ProductCustomUrl propValue = (ProductCustomUrl)prop.GetValue(request, null);
								string customMessage = String.Join("; ", Error.Errors.Select(e => e.Key == "custom_url" ? string.Format("'{0} = {1}' {2}", e.Key, propValue.Url, e.Value) : e.Value).ToArray());
								if (!string.IsNullOrEmpty(customMessage))
									return customMessage;
							}
						}
						catch { }
					}
				}
				else
				{
					if (!String.IsNullOrEmpty(Error.Title) && Error.Status == 422) //Missing fields		
					{
						string customMessage = String.Join(" ; ", Error.Errors.Select(e => string.Format("{0} {1}", e.Key, (e.Value == "error.path.missing" ? "is missing." : e.Value))).ToArray());
						if (!string.IsNullOrEmpty(customMessage))
							return customMessage;
					}
				}

			return error.ToString();
		}

		public static T TryDeserialize<T>(String content)
			where T : class
		{
			try
			{
				T result = JsonConvert.DeserializeObject<T>(content);
				return result;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
