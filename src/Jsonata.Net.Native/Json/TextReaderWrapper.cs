using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jsonata.Net.Native.Json;

internal sealed class TextReaderWrapper
{
    private readonly TextReader reader;
    private readonly char[] chars = new char[1024];
    private int charsCount = 0;
    private int firstCharIndex = 0;
    private bool endOfReaderReached = false;

    internal TextReaderWrapper(TextReader reader)
    {
        this.reader = reader;
    }

    internal async Task<int> PeekAsync(CancellationToken ct)
    {
        await this.AssureChars(ct);

        if (this.endOfReaderReached)
        {
            return -1;
        }

        return (this.chars[this.firstCharIndex]);
    }

    internal async Task<int> ReadAsync(CancellationToken ct)
    {
        await this.AssureChars(ct);

        if (this.endOfReaderReached)
        {
            return -1;
        }

        int result = this.chars[this.firstCharIndex];

        ++this.firstCharIndex;

        return result;
    }

    private async Task AssureChars(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        if (this.endOfReaderReached)
        {
            return;
        }

        if (this.firstCharIndex >= this.charsCount)
        {
            this.charsCount = await this.reader.ReadAsync(this.chars, 0, this.chars.Length);
            this.firstCharIndex = 0;
            if (this.charsCount == 0)
            {
                this.endOfReaderReached = true;
            }
        }
    }
}
