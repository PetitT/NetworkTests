using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FishingCactus.Util
{
    public static class StreamExtensions
    {
        public static async Task CopyToAsync( this Stream source, Stream destination, IProgress<char> progress, CancellationToken cancellationToken = default( CancellationToken ), int bufferSize = 0x1000 )
        {
            var buffer = new byte[ bufferSize ];
            int bytesRead;
            long currentRead = 0;
            long totalByteCount = source.Length;
            while ( ( bytesRead = await source.ReadAsync( buffer, 0, buffer.Length, cancellationToken ) ) > 0 )
            {
                await destination.WriteAsync( buffer, 0, bytesRead, cancellationToken );
                cancellationToken.ThrowIfCancellationRequested();
                currentRead += bytesRead;
                progress.Report( (char) ( currentRead * 100 / totalByteCount ) );
            }
        }
    }
}