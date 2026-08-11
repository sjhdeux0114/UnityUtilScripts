// =============================================================
// GifDecoder.cs — minimal runtime GIF89a decoder for Unity
//  - Reads animated GIF bytes → frames (Texture2D[]) + delays (sec)
//  - Supports: global/local color tables, transparency, interlace,
//              disposal methods 0,1,2,3 (RestorePrevious), loop count
//  - No external deps. Works on IL2CPP/AOT.
//  - MIT-like: feel free to use in your project.
// =============================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class GifFrames
{
    public Texture2D[] textures;   // RGBA32 frames
    public float[] delays;         // seconds per frame
    public int width;
    public int height;
    public int loopCount;          // 0 => infinite
}

public static class GifDecoder
{
    private struct GCE
    {
        public int disposal;      // 0,1,2,3
        public bool transparency;
        public byte transIndex;
        public int delayCs;       // delay in 1/100 sec
        public void Reset() { disposal = 0; transparency = false; transIndex = 0; delayCs = 0; }
    }

    private class Reader
    {
        public byte[] data; public int idx;
        public Reader(byte[] d) { data = d; idx = 0; }
        public bool EoF => idx >= data.Length;
        public byte ReadU8() { return data[idx++]; }
        public ushort ReadU16() { ushort v = (ushort)(data[idx] | (data[idx + 1] << 8)); idx += 2; return v; }
        public byte[] ReadN(int n) { var b = new byte[n]; Buffer.BlockCopy(data, idx, b, 0, n); idx += n; return b; }
        public void Skip(int n) { idx += n; }
    }

    private struct Color32Ex { public byte r, g, b, a; public Color32 ToC() => new Color32(r, g, b, a); }

    // GifDecoder.cs
    private static Color32[] FlipY(Color32[] src, int w, int h)
    {
        var dst = new Color32[src.Length];
        for (int y = 0; y < h; y++)
        {
            int srcRow = y * w;
            int dstRow = (h - 1 - y) * w;
            for (int x = 0; x < w; x++)
            {
                dst[dstRow + x] = src[srcRow + x];
            }
        }
        return dst;
    }



    public static GifFrames Decode(byte[] bytes)
    {
        var r = new Reader(bytes);
        // Header
        string sig = System.Text.Encoding.ASCII.GetString(r.ReadN(6));
        if (!(sig == "GIF87a" || sig == "GIF89a")) throw new Exception("Not a GIF file");

        int width = r.ReadU16();
        int height = r.ReadU16();
        byte packed = r.ReadU8();
        bool gctFlag = (packed & 0x80) != 0;
        int colorRes = ((packed >> 4) & 7) + 1; // unused
        bool sortFlag = (packed & 0x08) != 0; // unused
        int gctSize = 1 << ((packed & 0x07) + 1);
        byte bgIndex = r.ReadU8();
        byte pixelAspect = r.ReadU8(); // unused

        Color32Ex[] gct = null;
        if (gctFlag) gct = ReadColorTable(r, gctSize);

        var frames = new List<Texture2D>();
        var delays = new List<float>();

        // Canvas buffers
        var canvas = new Color32[width * height];
        for (int i = 0; i < canvas.Length; i++) canvas[i] = new Color32(0, 0, 0, 0);
        var previous = new Color32[canvas.Length];

        GCE gce = new GCE(); gce.Reset();
        int loopCount = 0; // 0 means infinite

        // Main blocks
        while (!r.EoF)
        {
            byte sep = r.ReadU8();
            if (sep == 0x3B) // Trailer
                break;

            if (sep == 0x21) // Extension
            {
                byte label = r.ReadU8();
                if (label == 0xF9) // GCE
                {
                    byte blockSize = r.ReadU8(); // should be 4
                    byte p = r.ReadU8();
                    gce.disposal = (p >> 2) & 7;
                    gce.transparency = (p & 1) != 0;
                    gce.delayCs = r.ReadU16();
                    gce.transIndex = r.ReadU8();
                    r.ReadU8(); // terminator 0x00
                }
                else if (label == 0xFF) // Application Extension
                {
                    byte blockSize = r.ReadU8(); // 11
                    string appId = System.Text.Encoding.ASCII.GetString(r.ReadN(blockSize));
                    // Read sub-blocks
                    while (true)
                    {
                        byte sz = r.ReadU8();
                        if (sz == 0) break;
                        var sub = r.ReadN(sz);
                        if (appId == "NETSCAPE2.0" && sz >= 3 && sub[0] == 1)
                        {
                            loopCount = sub[1] | (sub[2] << 8); // 0 = infinite
                        }
                    }
                }
                else // other extensions — skip subblocks
                {
                    while (true) { byte sz = r.ReadU8(); if (sz == 0) break; r.Skip(sz); }
                }
            }
            else if (sep == 0x2C) // Image Descriptor
            {
                int left = r.ReadU16();
                int top = r.ReadU16();
                int w = r.ReadU16();
                int h = r.ReadU16();
                byte pk = r.ReadU8();
                bool lctFlag = (pk & 0x80) != 0;
                bool interlaced = (pk & 0x40) != 0;
                int lctSize = 1 << ((pk & 0x07) + 1);

                var lct = lctFlag ? ReadColorTable(r, lctSize) : null;
                var palette = lct ?? gct;
                if (palette == null) throw new Exception("No color table found");

                // Save previous canvas for disposal=3
                Array.Copy(canvas, previous, canvas.Length);

                // LZW image data
                int lzwMinCodeSize = r.ReadU8();
                var imageData = ReadSubBlocks(r);
                var indices = LzwDecode(imageData, lzwMinCodeSize, w * h, interlaced, w, h);

                // Composite to canvas
                CompositeToCanvas(canvas, previous, indices, palette, width, height, left, top, w, h, gce);

                // Build Texture2D frame
                var tex = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;

                var flipped = FlipY(canvas, width, height);   // ← 세로 뒤집기
                tex.SetPixels32(flipped);
                tex.Apply(false, false);
                frames.Add(tex);


                float delaySec = Mathf.Max(0.002f, (gce.delayCs > 0 ? (gce.delayCs / 100f) : 0.1f));
                delays.Add(delaySec);

                // Apply disposal for next frame
                ApplyDisposal(canvas, previous, width, height, left, top, w, h, gce);

                // Reset GCE for next frame
                gce.Reset();
            }
            else
            {
                // Unknown block — try to continue
            }
        }

        var result = new GifFrames
        {
            textures = frames.ToArray(),
            delays = delays.ToArray(),
            width = width,
            height = height,
            loopCount = loopCount
        };
        return result;
    }

    private static Color32Ex[] ReadColorTable(Reader r, int size)
    {
        var table = new Color32Ex[size];
        for (int i = 0; i < size; i++)
        {
            byte r8 = r.ReadU8(); byte g8 = r.ReadU8(); byte b8 = r.ReadU8();
            table[i] = new Color32Ex { r = r8, g = g8, b = b8, a = 255 };
        }
        return table;
    }

    private static byte[] ReadSubBlocks(Reader r)
    {
        var list = new List<byte>(1024);
        while (true)
        {
            byte sz = r.ReadU8();
            if (sz == 0) break;
            var b = r.ReadN(sz);
            list.AddRange(b);
        }
        return list.ToArray();
    }

    // LZW decoder producing color indices; handles interlace by reordering rows after decode
    // GifDecoder.cs 안의 기존 LzwDecode(...)를 이 버전으로 교체
    private static byte[] LzwDecode(byte[] data, int minCodeSize, int expectedSize, bool interlaced, int w, int h)
    {
        // --- 초기화 ---
        int clearCode = 1 << minCodeSize;
        int endCode = clearCode + 1;
        int codeSize = minCodeSize + 1;
        int codeMask = (1 << codeSize) - 1;
        int avail = endCode + 1;
        int oldCode = -1;

        // 사전
        var prefix = new int[4096];
        var suffix = new byte[4096];
        var pixelStack = new byte[4097];

        for (int i = 0; i < clearCode; i++) { prefix[i] = 0; suffix[i] = (byte)i; }

        // 출력버퍼 (w*h == expectedSize)
        var outBuf = new byte[expectedSize];
        int outIdx = 0;

        // 비트 버퍼 (LSB first)
        int datum = 0;
        int bits = 0;
        int dataIdx = 0;

        int stackTop = 0;
        int first = 0;

        // --- 디코드 루프 ---
        while (outIdx < expectedSize)
        {
            // 코드 크기만큼 비트 채우기
            while (bits < codeSize)
            {
                if (dataIdx >= data.Length) goto decode_done;
                datum |= (data[dataIdx] & 0xFF) << bits; // LSB-first
                bits += 8;
                dataIdx++;
            }

            int code = datum & codeMask;
            datum >>= codeSize;
            bits -= codeSize;

            if (code == clearCode)
            {
                // 테이블 리셋
                codeSize = minCodeSize + 1;
                codeMask = (1 << codeSize) - 1;
                avail = endCode + 1;
                oldCode = -1;
                continue;
            }
            if (code == endCode) break;

            if (oldCode == -1)
            {
                // 첫 코드
                byte c = suffix[code];
                outBuf[outIdx++] = c;
                first = c;
                oldCode = code;
                continue;
            }

            int inCode = code;

            // KwKwK 특수 케이스: code == avail 인 경우 first를 푸시
            if (code >= avail)
            {
                pixelStack[stackTop++] = (byte)first;
                code = oldCode;
            }

            // 문자열 풀기
            while (code >= clearCode)
            {
                // 안전가드 (이상한 스트림 대비)
                if (code < 0 || code >= 4096 || stackTop >= pixelStack.Length) break;
                pixelStack[stackTop++] = suffix[code];
                code = prefix[code];
            }

            first = suffix[code];
            pixelStack[stackTop++] = (byte)first;

            // 새 문자열을 테이블에 추가
            if (avail < 4096)
            {
                prefix[avail] = oldCode;
                suffix[avail] = (byte)first;
                avail++;
                // 코드사이즈 증가 지점: avail == (1<<codeSize)
                if (avail == (1 << codeSize) && codeSize < 12)
                {
                    codeSize++;
                    codeMask = (1 << codeSize) - 1;
                }
            }

            oldCode = inCode;

            // 스택 팝 → 출력
            while (stackTop > 0 && outIdx < expectedSize)
            {
                outBuf[outIdx++] = pixelStack[--stackTop];
            }
        }

    decode_done:

        // 인터레이스 처리
        if (interlaced)
        {
            var deint = new byte[expectedSize];
            int[] offsets = { 0, 4, 2, 1 };
            int[] steps = { 8, 8, 4, 2 };
            int srcRow = 0;
            for (int pass = 0; pass < 4; pass++)
            {
                for (int y = offsets[pass]; y < h; y += steps[pass])
                {
                    if (srcRow >= h) break;
                    Buffer.BlockCopy(outBuf, srcRow * w, deint, y * w, w);
                    srcRow++;
                }
            }
            return deint;
        }

        return outBuf;
    }


    private static void CompositeToCanvas(Color32[] canvas, Color32[] previous, byte[] idxBuf,
                                          Color32Ex[] palette, int canvasW, int canvasH,
                                          int left, int top, int w, int h, GCE gce)
    {
        int trans = gce.transparency ? gce.transIndex : -1;
        for (int y = 0; y < h; y++)
        {
            int cy = top + y; if (cy < 0 || cy >= canvasH) continue;
            int cRow = cy * canvasW;
            int sRow = y * w;
            for (int x = 0; x < w; x++)
            {
                int cx = left + x; if (cx < 0 || cx >= canvasW) continue;
                int index = idxBuf[sRow + x];
                if (index == trans) continue; // keep previous pixel
                var col = palette[index];
                canvas[cRow + cx] = new Color32(col.r, col.g, col.b, 255);
            }
        }
    }

    private static void ApplyDisposal(Color32[] canvas, Color32[] previous, int canvasW, int canvasH,
                                      int left, int top, int w, int h, GCE gce)
    {
        switch (gce.disposal)
        {
            case 2: // restore to background (transparent)
                for (int y = 0; y < h; y++)
                {
                    int cy = top + y; if (cy < 0 || cy >= canvasH) continue;
                    int row = cy * canvasW;
                    for (int x = 0; x < w; x++)
                    {
                        int cx = left + x; if (cx < 0 || cx >= canvasW) continue;
                        canvas[row + cx] = new Color32(0, 0, 0, 0);
                    }
                }
                break;
            case 3: // restore to previous
                for (int y = 0; y < h; y++)
                {
                    int cy = top + y; if (cy < 0 || cy >= canvasH) continue;
                    int row = cy * canvasW;
                    for (int x = 0; x < w; x++)
                    {
                        int cx = left + x; if (cx < 0 || cx >= canvasW) continue;
                        canvas[row + cx] = previous[row + cx];
                    }
                }
                break;
            default:
                // 0 or 1: do nothing (leave as is)
                break;
        }
    }
}
