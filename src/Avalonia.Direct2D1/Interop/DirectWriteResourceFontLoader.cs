using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;

namespace Avalonia.Direct2D1.Interop;

[GeneratedComClass]
internal sealed unsafe partial class DWriteResourceFontLoader : IDWriteFontCollectionLoader, IDWriteFontFileLoader, IDisposable
{
    private readonly DWriteFactory _factory;
    private readonly List<byte[]> _fontData;
    private readonly List<DWriteResourceFontFileStream> _streams;
    private bool _isDisposed;

    public DWriteResourceFontLoader(DWriteFactory factory, IReadOnlyList<Stream> fontStreams)
    {
        _factory = factory;
        _fontData = new List<byte[]>(fontStreams.Count);
        _streams = new List<DWriteResourceFontFileStream>(fontStreams.Count);
        CollectionKey = new byte[fontStreams.Count * sizeof(int)];

        for (var i = 0; i < fontStreams.Count; i++)
        {
            using var memoryStream = new MemoryStream();
            fontStreams[i].CopyTo(memoryStream);

            _fontData.Add(memoryStream.ToArray());
            Buffer.BlockCopy(BitConverter.GetBytes(i), 0, CollectionKey, i * sizeof(int), sizeof(int));
        }

        _factory.Native.RegisterFontFileLoader(this);
        _factory.Native.RegisterFontCollectionLoader(this);
    }

    public byte[] CollectionKey { get; }

    public void CreateEnumeratorFromKey(
        IDWriteFactory factory,
        void* collectionKey,
        uint collectionKeySize,
        out IDWriteFontFileEnumerator fontFileEnumerator)
    {
        if (collectionKey is null || collectionKeySize == 0 || collectionKeySize % sizeof(int) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(collectionKeySize));
        }

        var key = new byte[collectionKeySize];
        Marshal.Copy((IntPtr)collectionKey, key, 0, key.Length);
        fontFileEnumerator = new DWriteResourceFontFileEnumerator(factory, this, key);
    }

    public void CreateStreamFromKey(
        void* fontFileReferenceKey,
        uint fontFileReferenceKeySize,
        out IDWriteFontFileStream fontFileStream)
    {
        if (fontFileReferenceKey is null || fontFileReferenceKeySize != sizeof(int))
        {
            throw new ArgumentOutOfRangeException(nameof(fontFileReferenceKeySize));
        }

        var index = *(int*)fontFileReferenceKey;

        if ((uint)index >= (uint)_fontData.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(fontFileReferenceKey));
        }

        var stream = new DWriteResourceFontFileStream(_fontData[index]);
        _streams.Add(stream);
        fontFileStream = stream;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        foreach (var stream in _streams)
        {
            stream.Dispose();
        }

        _streams.Clear();
        _factory.Native.UnregisterFontCollectionLoader(this);
        _factory.Native.UnregisterFontFileLoader(this);
    }
}

[GeneratedComClass]
internal sealed unsafe partial class DWriteResourceFontFileEnumerator : IDWriteFontFileEnumerator
{
    private readonly IDWriteFactory _factory;
    private readonly IDWriteFontFileLoader _loader;
    private readonly int[] _fontIndices;
    private int _currentIndex = -1;

    public DWriteResourceFontFileEnumerator(IDWriteFactory factory, IDWriteFontFileLoader loader, byte[] collectionKey)
    {
        _factory = factory;
        _loader = loader;
        _fontIndices = new int[collectionKey.Length / sizeof(int)];
        Buffer.BlockCopy(collectionKey, 0, _fontIndices, 0, collectionKey.Length);
    }

    public void MoveNext(BOOL* hasCurrentFile)
    {
        if (hasCurrentFile is null)
        {
            throw new ArgumentNullException(nameof(hasCurrentFile));
        }

        if (_currentIndex + 1 < _fontIndices.Length)
        {
            _currentIndex++;
            *hasCurrentFile = true;
            return;
        }

        *hasCurrentFile = false;
    }

    public void GetCurrentFontFile(out IDWriteFontFile fontFile)
    {
        if ((uint)_currentIndex >= (uint)_fontIndices.Length)
        {
            throw new InvalidOperationException("The font file enumerator is not positioned on a valid font.");
        }

        var key = BitConverter.GetBytes(_fontIndices[_currentIndex]);
        fixed (byte* keyPtr = key)
        {
            _factory.CreateCustomFontFileReference(keyPtr, (uint)key.Length, _loader, out fontFile);
        }
    }
}

[GeneratedComClass]
internal sealed unsafe partial class DWriteResourceFontFileStream : IDWriteFontFileStream, IDisposable
{
    private readonly byte[] _fontData;
    private readonly GCHandle _handle;
    private bool _isDisposed;

    public DWriteResourceFontFileStream(byte[] fontData)
    {
        _fontData = fontData;
        _handle = GCHandle.Alloc(_fontData, GCHandleType.Pinned);
    }

    public void ReadFileFragment(void** fragmentStart, ulong fileOffset, ulong fragmentSize, void** fragmentContext)
    {
        if (fragmentStart is null)
        {
            throw new ArgumentNullException(nameof(fragmentStart));
        }

        if (fragmentContext is null)
        {
            throw new ArgumentNullException(nameof(fragmentContext));
        }

        if (fileOffset > (ulong)_fontData.LongLength || fragmentSize > (ulong)_fontData.LongLength - fileOffset)
        {
            throw new ArgumentOutOfRangeException(nameof(fileOffset));
        }

        if (fileOffset > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(fileOffset));
        }

        *fragmentStart = (byte*)_handle.AddrOfPinnedObject() + (int)fileOffset;
        *fragmentContext = null;
    }

    public void ReleaseFileFragment(void* fragmentContext)
    {
    }

    public void GetFileSize(out ulong fileSize)
    {
        fileSize = (ulong)_fontData.LongLength;
    }

    public void GetLastWriteTime(out ulong lastWriteTime)
    {
        lastWriteTime = 0;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (_handle.IsAllocated)
        {
            _handle.Free();
        }
    }
}

internal sealed class DWriteCustomFontCollection : IDisposable
{
    private readonly DWriteResourceFontLoader _loader;
    private bool _isDisposed;

    public DWriteCustomFontCollection(DWriteFontCollection fontCollection, DWriteResourceFontLoader loader)
    {
        FontCollection = fontCollection;
        _loader = loader;
    }

    public DWriteFontCollection FontCollection { get; }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        FontCollection.Dispose();
        _loader.Dispose();
    }
}
