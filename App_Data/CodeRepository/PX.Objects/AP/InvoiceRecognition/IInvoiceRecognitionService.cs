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
using JetBrains.Annotations;
using PX.CloudServices.DocumentRecognition;

namespace PX.Objects.AP.InvoiceRecognition
{
    public interface IInvoiceRecognitionService
    {
        [ItemNotNull]
        Task<DocumentRecognitionResponse> SendFile(Guid fileId, [NotNull] byte[] file, [NotNull] string contentType, CancellationToken cancellationToken);
        [ItemNotNull]
        Task<DocumentRecognitionResponse> GetResult([NotNull] string state, CancellationToken cancellationToken);

        bool IsConfigured();
        [NotNull]
        string ModelName { get; }
    }

    /// <remarks>
    /// Can be in one of two states:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <see cref="State"/> is not <see langword="null"/>: the recognition is in progress
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="Result"/> is not <see langword="null"/>: the recognition has completed successfully
    /// </description>
    /// </item>
    /// </list>
    /// The other property is always <see langword="null"/>.
    /// </remarks>
    public sealed class DocumentRecognitionResponse
    {
        public DocumentRecognitionResponse([NotNull] string state)
            => State = state ?? throw new ArgumentNullException(nameof(state));

        public DocumentRecognitionResponse([NotNull] DocumentRecognitionResult result)
            => Result = result ?? throw new ArgumentNullException(nameof(result));

        [CanBeNull]
        public string State { get; }
        [CanBeNull]
        public DocumentRecognitionResult Result { get; }

        public void Deconstruct([CanBeNull] out DocumentRecognitionResult result, [CanBeNull] out string state)
        {
            result = Result;
            state = State;
        }
    }

    internal interface IInvoiceRecognitionFeedback
    {
        Task Send([NotNull] Uri address, [NotNull] HttpContent content, CancellationToken cancellationToken = default);
    }
}
