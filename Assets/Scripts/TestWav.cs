using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class TestWav
{
    const int HEADER_SIZE = 44;

    public static bool Save(string filename, List<AudioClip> clips)
    {
        if (!filename.ToLower().EndsWith(".wav"))
            filename += ".wav";

        string filepath = Path.Combine(Application.persistentDataPath, filename);
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        Debug.Log("Saving WAV: " + filepath);

        using (var fileStream = CreateEmpty(filepath))
        {
            ConvertAndWrite(fileStream, clips);
            WriteHeader(fileStream, clips);
        }

        return true;
    }

    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        for (int i = 0; i < HEADER_SIZE; i++)
            fileStream.WriteByte(0);
        return fileStream;
    }

    static void ConvertAndWrite(FileStream fileStream, List<AudioClip> clips)
    {
        // Calculate total samples and check consistency
        int totalSamples = 0;
        int frequency = clips[0].frequency;
        int channels = clips[0].channels;

        foreach (var clip in clips)
        {
            if (clip.channels != channels || clip.frequency != frequency)
                Debug.LogWarning($"Clip '{clip.name}' has different format — may cause issues!");
            totalSamples += clip.samples * clip.channels;
        }

        float[] allSamples = new float[totalSamples];
        int offset = 0;

        foreach (var clip in clips)
        {
            float[] clipSamples = new float[clip.samples * clip.channels];
            clip.GetData(clipSamples, 0);

            Array.Copy(clipSamples, 0, allSamples, offset, clipSamples.Length);
            offset += clipSamples.Length;
        }

        short[] intData = new short[allSamples.Length];
        byte[] bytesData = new byte[allSamples.Length * 2];
        int rescaleFactor = 32767;

        for (int i = 0; i < allSamples.Length; i++)
        {
            intData[i] = (short)(allSamples[i] * rescaleFactor);
            BitConverter.GetBytes(intData[i]).CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    static void WriteHeader(FileStream fileStream, List<AudioClip> clips)
    {
        int frequency = clips[0].frequency;
        int channels = clips[0].channels;
        int totalSamples = 0;

        foreach (var clip in clips)
            totalSamples += clip.samples * clip.channels;

        fileStream.Seek(0, SeekOrigin.Begin);

        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        fileStream.Write(BitConverter.GetBytes(fileStream.Length - 8), 0, 4);
        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        fileStream.Write(BitConverter.GetBytes(16), 0, 4);
        fileStream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
        fileStream.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
        fileStream.Write(BitConverter.GetBytes(frequency), 0, 4);
        fileStream.Write(BitConverter.GetBytes(frequency * channels * 2), 0, 4);
        fileStream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
        fileStream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        fileStream.Write(BitConverter.GetBytes(totalSamples * 2), 0, 4);

        fileStream.Close();
    }
}
