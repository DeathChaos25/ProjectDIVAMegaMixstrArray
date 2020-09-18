using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaMixstrArray
{
    class strArrayFile
    {
        public List<String> strArrayMessages { get; private set; }
        public uint[] strArrayStringPointers { get; set; }
        Encoding utf8WithoutBom = new UTF8Encoding(false);


        public void ReadstrArray(Stream strArray_path)
        {
            using (EndianBinaryReader strArray = new EndianBinaryReader(strArray_path, Encoding.GetEncoding(65001), true, Endianness.Little))
            {
                int numOfPointers = 0;
                strArray.Seek(0, SeekOrigin.Begin);// file has no header, literally just pointers and text
                for (int i = 0; i < 100000; i++) // the game code reads pointers until it hits a 00 00 00 00
                {
                    var tempPointer = strArray.ReadUInt32();
                    if (tempPointer > 0)
                    {
                        numOfPointers++;
                    }
                    else i = 99999999; // no header means no defined number of pointers, so we use this to break;
                };
                // Console.WriteLine("numOfPointers = " + numOfPointers.ToString());
                strArray.Seek(0, SeekOrigin.Begin);// reset back to beginning after reading pointers

                strArrayStringPointers = new uint[numOfPointers];
                strArrayMessages = new List<String>();

                ///store all strings in strArray on List
                for (int i = 0; i < numOfPointers; i++) 
                {
                    strArrayStringPointers[i] = strArray.ReadUInt32();
                    // Console.WriteLine("Pointer # " + i.ToString()+ " points to " + strArrayStringPointers[i].ToString());
                }
                for (int i = 0; i < numOfPointers; i++)
                {
                    strArray.Seek(strArrayStringPointers[i], SeekOrigin.Begin);
                    strArrayMessages.Add(StripControlChars(strArray.ReadString(StringBinaryFormat.NullTerminated)));
                }
            }
        }
        string StripControlChars(string arg)
        {
            char[] arrForm = arg.ToCharArray();
            StringBuilder buffer = new StringBuilder(arg.Length);//This many chars at most

            foreach (char ch in arrForm)
            {
                if (!Char.IsControl(ch))
                {
                    buffer.Append(ch);//Only add to buffer if not a control char
                }
                else buffer.Append(@"[" + Convert.ToByte(ch).ToString("x2") + "]");
            }
            return buffer.ToString();
        }
        public void WritestrArray(EndianBinaryWriter newstrArray, List<string> strArrayStrings)
        {
            newstrArray.PushBaseOffset(0);
            byte newLine = 0xa;
            byte escapeChar = 0x1b;

            foreach (string stringLine in strArrayStrings)
            {
                var newStringLine = String.Empty;
                if (stringLine.Contains("[0a]"))
                    newStringLine = stringLine.Replace("[0a]", "@").Replace('@', (char)newLine);
                else if (stringLine.Contains("[1b]"))
                    newStringLine = stringLine.Replace("[1b]", "@").Replace('@', (char)escapeChar);
                else
                    newStringLine = stringLine;

                newstrArray.ScheduleWriteOffsetAligned(0, () => newstrArray.WriteString(newStringLine, StringBinaryFormat.NullTerminated));
            }
            newstrArray.WriteInt32(0); // write 00 00 00 00 at the end of pointer list so game knows to stop reading pointers
            if (((0x10 - newstrArray.Position % 0x10) % 0x10) > 0)
                newstrArray.WritePadding((int)((0x10 - newstrArray.Position % 0x10) % 0x10)); // not needed but official files pad the entire line, so we do too
            // Console.WriteLine("Bytes needed to align: " + ((0x10 - newstrArray.Position % 0x10) % 0x10).ToString());
            newstrArray.PopBaseOffset();

            // This performs the actual scheduled writes recursively, meaning it will go down the chain to resolve nested calls
            newstrArray.PerformScheduledWrites();
            newstrArray.Close();
        }
    }
}
