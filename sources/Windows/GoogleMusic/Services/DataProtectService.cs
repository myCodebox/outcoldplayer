// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Threading.Tasks;

    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;
    using Windows.Security.Cryptography.DataProtection;
    using Windows.Storage.Streams;

    public class DataProtectService : IDataProtectService
    {
        private const BinaryStringEncoding Encoding = BinaryStringEncoding.Utf8;

        public async Task<string> ProtectStringAsync(string unprotectedString)
        {
            var provider = this.GetDataProtectionProvider();

            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(unprotectedString, Encoding);

            IBuffer buffProtectedData = null;

            // Create a random access stream to contain the plaintext message.
            using (InMemoryRandomAccessStream inputData = new InMemoryRandomAccessStream())
            using (InMemoryRandomAccessStream protectedData = new InMemoryRandomAccessStream())
            {
                IOutputStream outputStream = inputData.GetOutputStreamAt(0);
                using (DataWriter writer = new DataWriter(outputStream))
                {
                    writer.WriteBuffer(buffMsg);
                    await writer.StoreAsync();
                    await outputStream.FlushAsync();
                }

                IInputStream source = inputData.GetInputStreamAt(0);

                IOutputStream dest = protectedData.GetOutputStreamAt(0);
                await provider.ProtectStreamAsync(source, dest);
                await dest.FlushAsync();

                using (DataReader reader = new DataReader(protectedData.GetInputStreamAt(0)))
                {
                    await reader.LoadAsync((uint)protectedData.Size);
                    buffProtectedData = reader.ReadBuffer((uint)protectedData.Size);
                }
            }

            return Convert.ToBase64String(buffProtectedData.ToArray());
        }

        public async Task<string> UnprotectStringAsync(string protectedString)
        {
            var provider = this.GetDataProtectionProvider();

            IBuffer buffUnprotectedData;

            using (InMemoryRandomAccessStream unprotectedData = new InMemoryRandomAccessStream())
            using (InMemoryRandomAccessStream inputData = new InMemoryRandomAccessStream())
            {
                IOutputStream outputStream = inputData.GetOutputStreamAt(0);
                using (DataWriter writer = new DataWriter(outputStream))
                {
                    writer.WriteBuffer(Convert.FromBase64String(protectedString).AsBuffer());
                    await writer.StoreAsync();
                    await outputStream.FlushAsync();
                }

                IInputStream source = inputData.GetInputStreamAt(0);

                IOutputStream dest = unprotectedData.GetOutputStreamAt(0);
                await provider.UnprotectStreamAsync(source, dest);
                await dest.FlushAsync();

                using (DataReader reader = new DataReader(unprotectedData.GetInputStreamAt(0)))
                {
                    await reader.LoadAsync((uint)unprotectedData.Size);
                    buffUnprotectedData = reader.ReadBuffer((uint)unprotectedData.Size);
                }
            }

            return CryptographicBuffer.ConvertBinaryToString(Encoding, buffUnprotectedData);
        }

        public string GetMd5HashStringAsBase64(string content)
        {
            HashAlgorithmProvider hashProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer hash =
                hashProvider.HashData(
                    CryptographicBuffer.ConvertStringToBinary(content, BinaryStringEncoding.Utf8));
            return CryptographicBuffer.EncodeToBase64String(hash);
        }

        public byte[] GetMd5Hash(string content)
        {
            HashAlgorithmProvider hashProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer hash =
                hashProvider.HashData(
                    CryptographicBuffer.ConvertStringToBinary(content, BinaryStringEncoding.Utf8));
            return hash.ToArray();
        }

        public string GetHMacStringAsBase64(string key, string value)
        {
            var provider = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
            var hmacKey = provider.CreateKey(CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8));
            var signedData = CryptographicEngine.Sign(hmacKey, CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8));

            return Convert.ToBase64String(signedData.ToArray());
        }

        private DataProtectionProvider GetDataProtectionProvider()
        {
            return new DataProtectionProvider("LOCAL=user");
        }
    }
}