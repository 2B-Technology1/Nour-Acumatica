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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PX.CloudServices.DocumentRecognition;

namespace PX.Objects.AP.InvoiceRecognition
{
    internal class InvoiceRecognitionService : IInvoiceRecognitionService, IInvoiceRecognitionFeedback
    {
        private readonly string ModelName;

        private readonly IDocumentRecognitionClient _documentRecognitionClient;

        public InvoiceRecognitionService(IDocumentRecognitionClient documentRecognitionClient,
			IOptions<InvoiceRecognitionModelOptions> modelOptions)
        {
            _documentRecognitionClient = documentRecognitionClient;
			ModelName = modelOptions.Value.Name;
        }

        string IInvoiceRecognitionService.ModelName => ModelName;

        bool IInvoiceRecognitionService.IsConfigured() => _documentRecognitionClient.IsConfigured();

        async Task<DocumentRecognitionResponse> IInvoiceRecognitionService.SendFile(Guid fileId, byte[] file, string contentType, CancellationToken cancellationToken) =>
            new DocumentRecognitionResponse(
                ToState(await _documentRecognitionClient.SendFile(ModelName, fileId, file, contentType, cancellationToken))
            );

        async Task<DocumentRecognitionResponse> IInvoiceRecognitionService.GetResult(string state, CancellationToken cancellationToken)
        {
            var (result, uri) = await _documentRecognitionClient.GetResult(FromState(state), cancellationToken);
            if (result != null)
                return new DocumentRecognitionResponse(result);
            if (uri != null)
                return new DocumentRecognitionResponse(ToState(uri));
            throw new InvalidOperationException($"The result from {nameof(IDocumentRecognitionClient.GetResult)} is completely empty");
        }

        private static Uri FromState(string state) => new Uri(state);
        private static string ToState(Uri uri) => uri.ToString();

        Task IInvoiceRecognitionFeedback.Send(Uri address, HttpContent content, CancellationToken cancellationToken) =>
            _documentRecognitionClient.Feedback(address, content, cancellationToken);
    }
}