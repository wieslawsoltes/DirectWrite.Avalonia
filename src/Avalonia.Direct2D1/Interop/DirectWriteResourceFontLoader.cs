using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Avalonia.Direct2D1.Interop;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
internal sealed class DWriteResourceFontLoader : IDWriteFontCollectionLoader, IDWriteFontFileLoader, IDisposable
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

        HResult.ThrowIfFailed(_factory.Native.RegisterFontFileLoader(this));
        HResult.ThrowIfFailed(_factory.Native.RegisterFontCollectionLoader(this));
    }

    public byte[] CollectionKey { get; }

    public int CreateEnumeratorFromKey(
        IDWriteFactory factory,
        IntPtr collectionKey,
        uint collectionKeySize,
        out IDWriteFontFileEnumerator? fontFileEnumerator)
    {
        fontFileEnumerator = null;

        if (collectionKey == IntPtr.Zero || collectionKeySize == 0 || collectionKeySize % sizeof(int) != 0)
        {
            return HResult.E_INVALIDARG;
        }

        var key = new byte[collectionKeySize];
        Marshal.Copy(collectionKey, key, 0, key.Length);
        fontFileEnumerator = new DWriteResourceFontFileEnumerator(factory, this, key);
        return HResult.S_OK;
    }

    public int CreateStreamFromKey(
        IntPtr fontFileReferenceKey,
        uint fontFileReferenceKeySize,
        out IDWriteFontFileStream? fontFileStream)
    {
        fontFileStream = null;

        if (fontFileReferenceKey == IntPtr.Zero || fontFileReferenceKeySize != sizeof(int))
        {
            return HResult.E_INVALIDARG;
        }

        var index = Marshal.ReadInt32(fontFileReferenceKey);

        if ((uint)index >= (uint)_fontData.Count)
        {
            return HResult.E_INVALIDARG;
        }

        var stream = new DWriteResourceFontFileStream(_fontData[index]);
        _streams.Add(stream);
        fontFileStream = stream;
        return HResult.S_OK;
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

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
internal sealed class DWriteResourceFontFileEnumerator : IDWriteFontFileEnumerator
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

    public int MoveNext(out bool hasCurrentFile)
    {
        if (_currentIndex + 1 < _fontIndices.Length)
        {
            _currentIndex++;
            hasCurrentFile = true;
            return HResult.S_OK;
        }

        hasCurrentFile = false;
        return HResult.S_OK;
    }

    public int GetCurrentFontFile(out IDWriteFontFile? fontFile)
    {
        fontFile = null;

        if ((uint)_currentIndex >= (uint)_fontIndices.Length)
        {
            return HResult.E_FAIL;
        }

        var index = _fontIndices[_currentIndex];
        var key = BitConverter.GetBytes(index);
        var handle = GCHandle.Alloc(key, GCHandleType.Pinned);

        try
        {
            return _factory.CreateCustomFontFileReference(
                handle.AddrOfPinnedObject(),
                (uint)key.Length,
                _loader,
                out fontFile);
        }
        finally
        {
            handle.Free();
        }
    }
}

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
internal sealed class DWriteResourceFontFileStream : IDWriteFontFileStream, IDisposable
{
    private readonly byte[] _fontData;
    private readonly GCHandle _handle;
    private bool _isDisposed;

    public DWriteResourceFontFileStream(byte[] fontData)
    {
        _fontData = fontData;
        _handle = GCHandle.Alloc(_fontData, GCHandleType.Pinned);
    }

    public int ReadFileFragment(out IntPtr fragmentStart, ulong fileOffset, ulong fragmentSize, out IntPtr fragmentContext)
    {
        fragmentStart = IntPtr.Zero;
        fragmentContext = IntPtr.Zero;

        if (fileOffset > (ulong)_fontData.LongLength || fragmentSize > (ulong)_fontData.LongLength - fileOffset)
        {
            return HResult.E_INVALIDARG;
        }

        if (fileOffset > int.MaxValue)
        {
            return HResult.E_INVALIDARG;
        }

        fragmentStart = _handle.AddrOfPinnedObject() + (int)fileOffset;
        return HResult.S_OK;
    }

    public void ReleaseFileFragment(IntPtr fragmentContext)
    {
    }

    public int GetFileSize(out ulong fileSize)
    {
        fileSize = (ulong)_fontData.LongLength;
        return HResult.S_OK;
    }

    public int GetLastWriteTime(out ulong lastWriteTime)
    {
        lastWriteTime = 0;
        return HResult.S_OK;
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
