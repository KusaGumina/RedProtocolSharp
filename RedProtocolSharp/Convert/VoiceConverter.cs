using System.Net;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace RedProtocolSharp.Convert;

public class VoiceConverter
{
    public class VoiceInfo
    {
        public string name { get; set; } = "";
        public string filePath { get; set; } = "";
        public int duration { get; set; }
    }
    [DllImport("SilkRsDll.dll",EntryPoint = "encode",CallingConvention = CallingConvention.Cdecl)]
    public static extern void encode(IntPtr input_path, IntPtr output_path, Int32 sample_rate);
    
    public static VoiceInfo Mp3ToSilk(string inputFilePath)
    {
        var voiceInfo = new VoiceInfo();
        if (!File.Exists(inputFilePath)) return null;
        string fileName = Path.GetFileName(inputFilePath);
        voiceInfo.name = fileName;
        string tempFilePath = AppDomain.CurrentDomain.BaseDirectory + @"Cache\RedProtocolSharp"+fileName;
        string outFilePath = AppDomain.CurrentDomain.BaseDirectory + @"Cache\RedProtocolSharp"+fileName+".amr";
        voiceInfo.filePath = outFilePath;
        int targetSampleRate = 12000;

        using (var reader = new Mp3FileReader(inputFilePath))
        {
            voiceInfo.duration = (int)reader.TotalTime.TotalSeconds;
            var outFormat = new WaveFormat(targetSampleRate, reader.Mp3WaveFormat.Channels);
            using (var resampler = new MediaFoundationResampler(reader, outFormat))
            {
                WaveFileWriter.CreateWaveFile(tempFilePath, resampler);
            }
        }
        int sampleRate = 24000;
        IntPtr inputPtr = Marshal.StringToCoTaskMemUTF8(tempFilePath);
        IntPtr outputPtr = Marshal.StringToCoTaskMemUTF8(outFilePath);
        encode(inputPtr, outputPtr, sampleRate);
        Marshal.FreeHGlobal(inputPtr);
        Marshal.FreeHGlobal(outputPtr);
        File.Delete(tempFilePath);
        return !File.Exists(outFilePath) ? null : voiceInfo;
    }
}