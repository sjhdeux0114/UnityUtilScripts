using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class SaveScreenShot : MonoBehaviour
{
    public KeyCode key = KeyCode.LeftControl;
    public KeyCode Copykey = KeyCode.LeftAlt;
    int cnt = 0;

    // Win32 Clipboard API Imports
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool CloseClipboard();

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GlobalAlloc(uint uFlags, IntPtr dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GlobalFree(IntPtr hMem);

    const uint CF_DIB = 8;
    const uint GMEM_MOVEABLE = 0x0002;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(key))
            {
                ScreenCapture.CaptureScreenshot("shot" + System.DateTime.Now.Ticks + ".png");
                Debug.Log("shot" + System.DateTime.Now.Ticks + ".png");
                cnt++;
            }

            if (Input.GetKeyDown(Copykey))
            {
                StartCoroutine(CopyScreenshotToClipboardCoroutine());
            }
        }
    }

    IEnumerator CopyScreenshotToClipboardCoroutine()
    {
        // Wait until the frame rendering is complete
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read pixels from screen
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        try
        {
            CopyTextureToClipboard(tex);
            Debug.Log("Screenshot copied to clipboard successfully!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to copy screenshot to clipboard: " + ex.Message);
        }

        Destroy(tex);
    }

    void CopyTextureToClipboard(Texture2D tex)
    {
        int width = tex.width;
        int height = tex.height;
        Color32[] pixels = tex.GetPixels32();

        int headerSize = 40;
        int pixelSize = width * height * 4;
        int totalSize = headerSize + pixelSize;

        // Allocate moveable global memory
        IntPtr hMem = GlobalAlloc(GMEM_MOVEABLE, (IntPtr)totalSize);
        if (hMem == IntPtr.Zero)
        {
            throw new Exception("GlobalAlloc failed to allocate memory.");
        }

        IntPtr pMem = GlobalLock(hMem);
        if (pMem == IntPtr.Zero)
        {
            GlobalFree(hMem);
            throw new Exception("GlobalLock failed to lock memory.");
        }

        // Write BITMAPINFOHEADER (Device Independent Bitmap)
        byte[] header = new byte[headerSize];
        BitConverter.GetBytes(headerSize).CopyTo(header, 0);       // biSize
        BitConverter.GetBytes(width).CopyTo(header, 4);            // biWidth
        BitConverter.GetBytes(height).CopyTo(header, 8);           // biHeight
        BitConverter.GetBytes((short)1).CopyTo(header, 12);        // biPlanes
        BitConverter.GetBytes((short)32).CopyTo(header, 14);       // biBitCount
        BitConverter.GetBytes(0).CopyTo(header, 16);               // biCompression (BI_RGB)
        BitConverter.GetBytes(pixelSize).CopyTo(header, 20);       // biSizeImage
        BitConverter.GetBytes(0).CopyTo(header, 24);               // biXPelsPerMeter
        BitConverter.GetBytes(0).CopyTo(header, 28);               // biYPelsPerMeter
        BitConverter.GetBytes(0).CopyTo(header, 32);               // biClrUsed
        BitConverter.GetBytes(0).CopyTo(header, 36);               // biClrImportant

        // Copy header to global memory
        Marshal.Copy(header, 0, pMem, headerSize);

        // Convert Unity RGBA pixels to Win32 BGRA pixels
        byte[] rawPixels = new byte[pixelSize];
        int idx = 0;
        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 pixel = pixels[i];
            rawPixels[idx++] = pixel.b; // B
            rawPixels[idx++] = pixel.g; // G
            rawPixels[idx++] = pixel.r; // R
            rawPixels[idx++] = pixel.a; // A
        }

        // Copy pixel data to global memory right after the header
        IntPtr pPixels = new IntPtr(pMem.ToInt64() + headerSize);
        Marshal.Copy(rawPixels, 0, pPixels, pixelSize);

        GlobalUnlock(hMem);

        // Open Clipboard and set the data
        if (OpenClipboard(IntPtr.Zero))
        {
            EmptyClipboard();
            IntPtr handle = SetClipboardData(CF_DIB, hMem);
            CloseClipboard();

            if (handle == IntPtr.Zero)
            {
                // If SetClipboardData fails, we must free the memory block
                GlobalFree(hMem);
                throw new Exception("SetClipboardData failed.");
            }
        }
        else
        {
            GlobalFree(hMem);
            throw new Exception("OpenClipboard failed.");
        }
    }
}
