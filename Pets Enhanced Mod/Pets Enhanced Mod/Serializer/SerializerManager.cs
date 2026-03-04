using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO.Compression;
using static Pets_Enhanced_Mod.Multiplayer.SynchronizationManager;
using System.Runtime.InteropServices;

namespace Pets_Enhanced_Mod.Serializer
{
    public class SerializerManager
    {
        public static string SerializePets(List<PetInformation> pets)
        {
            if (pets == null || pets.Count == 0) return string.Empty;

            int byteCount = pets.Count * Marshal.SizeOf<PetInformation>();

            int base64Length = ((byteCount + 2) / 3) * 4;

            char[] charArray = ArrayPool<char>.Shared.Rent(base64Length);
            Span<char> charSpan = charArray.AsSpan(0, base64Length);

            try
            {
                ReadOnlySpan<PetInformation> collection = CollectionsMarshal.AsSpan(pets).Slice(0, pets.Count);
                ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(collection);

                if (Convert.TryToBase64Chars(byteSpan, charSpan, out int charsWritten))
                {
                    return new string(charSpan);
                }

                return string.Empty;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(charArray);
            }
        }
        public static ReadOnlySpan<PetInformation> DeserializeInto(ReadOnlySpan<char> base64Data)
        {
            var result = new ReadOnlySpan<PetInformation>();
            if (base64Data.IsEmpty) return result;

            int byteCount = (base64Data.Length * 3) / 4;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);

            try
            {
                if (Convert.TryFromBase64Chars(base64Data, buffer, out int bytesWritten))
                {
                    Span<byte> decodedBytes = buffer.AsSpan(0, bytesWritten);

                    result = MemoryMarshal.Cast<byte, PetInformation>(decodedBytes);

                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            return result;
        }
    }
}
