﻿//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudioTools {
    /// <summary>
    /// Wraps a <see cref="WebSocket"/> instance and exposes its interface as a generic <see cref="Stream"/>.
    /// </summary>
    internal class WebSocketStream : Stream {
        private readonly WebSocket _webSocket;
        private bool _ownsSocket;

        public WebSocketStream(WebSocket webSocket, bool ownsSocket = false) {
            _webSocket = webSocket;
            _ownsSocket = ownsSocket;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing && _ownsSocket) {
                _webSocket.Dispose();
            }
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override void Flush() {
        }

        public override Task FlushAsync(CancellationToken cancellationToken) {
            return Task.FromResult(true);
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            try {
                return (await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), cancellationToken).ConfigureAwait(false)).Count;
            } catch (WebSocketException ex) {
                throw new IOException(ex.Message, ex);
            }
        }

        public override void Write(byte[] buffer, int offset, int count) {
            WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            try {
                await _webSocket.SendAsync(new ArraySegment<byte>(buffer, offset, count), WebSocketMessageType.Binary, true, cancellationToken).ConfigureAwait(false);
            } catch (WebSocketException ex) {
                throw new IOException(ex.Message, ex);
            }
        }

        public override long Length {
            get { throw new NotSupportedException(); }
        }

        public override long Position {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }
    }
}
