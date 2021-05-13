﻿//  Copyright © 2021 by Threshold Secrets Llc.
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using Goedel.Utilities;
//using Goedel.Protocol.Service;
using System.Threading.Tasks;



namespace Goedel.Protocol.Presentation {
    /// <summary>
    /// RUD stream sender class. Provides methods to send datagrams and to receive
    /// notice of receipt of a control message.
    /// </summary>
    public class RudStreamSender: RudStream {
        #region // Constructors

        /// <summary>
        /// Initialize a new stream instance as a child of <paramref name="parent"/> to support
        /// protocol <paramref name="protocol"/> 
        /// </summary>
        /// <param name="parent">The parent stream</param>
        /// <param name="protocol">The stream protocol</param>
        /// <param name="credential">Optional additional credential.</param>

        public RudStreamSender(
                RudStream parent,
                string protocol,
                ICredentialPrivate credential =null,
                    string accountAddress = null) : base(parent, protocol, credential, accountAddress) {


            }
        #endregion


        #region // Methods

        /// <summary>
        /// Asynchronous method that waits for receipt of a control message.
        /// </summary>
        /// <returns>The datagram received.</returns>
        public async Task<DataGram> AsyncControlDatagram() => throw new NYI();

        /// <summary>
        /// Asynchronous message to send a datagram.
        /// </summary>
        /// <param name="dataGram">The datagram to send.</param>
        public async void Send(DataGram dataGram) {

            }
        #endregion

        }


    }