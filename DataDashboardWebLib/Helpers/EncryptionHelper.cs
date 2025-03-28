using System.Security.Cryptography;

namespace DataDashboardWebLib.Helpers
{
    public class EncryptionHelper
    {
        //private RSAParameters PublicKey;
        //private RSAParameters PrivateKey;

        private string CONTAINER_NAME = "DataDashboard";

        public enum KeySizes
        {
            SIZE_512 = 512,
            SIZE_1024 = 1024,
            SIZE_2048 = 2048,
            SIZE_952 = 952,
            SIZE_1369 = 1369
        };

        public EncryptionHelper()
        {
            GenerateKeys();
        }

        private void GenerateKeys()
        {
            int rsa_provider = 1;

            // 1 for rsa ; 13 for DSA ( Digital signature algorithm)
            // Generate new keys (or use existing ones) and store them in a container
            // Container is used because often encrypt and decrypt are in different page requests
            CspParameters cspParameters = new CspParameters(rsa_provider)
            {
                KeyContainerName = CONTAINER_NAME,
                Flags = CspProviderFlags.UseExistingKey | CspProviderFlags.UseMachineKeyStore,
                ProviderName = "Microsoft Strong Cryptographic Provider"
            };

            var rsa = new RSACryptoServiceProvider(cspParameters)
            {
                PersistKeyInCsp = true
            };
        }

        private void DeleteKeyInCSP()
        {
            // Delete the keys from this container
            var cspParameters = new CspParameters
            {
                KeyContainerName = CONTAINER_NAME
            };

            var rsa = new RSACryptoServiceProvider(cspParameters)
            {
                PersistKeyInCsp = false
            };

            rsa.Clear();
        }

        public byte[] Encrypt(byte[] plain)
        {
            // Encrypt the passed in data using the keys in the container
            byte[] encrypted;
            int rsa_provider = 1;

            CspParameters cspParameters = new CspParameters(rsa_provider)
            {
                KeyContainerName = CONTAINER_NAME
            };

            using (var rsa = new RSACryptoServiceProvider((int)KeySizes.SIZE_2048, cspParameters))
            {
                encrypted = rsa.Encrypt(plain, true);
            }

            return encrypted;
        }

        public byte[] Decrypt(byte[] encrypted)
        {
            // Decrypte the passed in data using the keys in the container
            byte[] decrypted;

            CspParameters cspParameters = new CspParameters
            {
                KeyContainerName = CONTAINER_NAME
            };

            using (var rsa = new RSACryptoServiceProvider((int)KeySizes.SIZE_2048, cspParameters))
            {
                decrypted = rsa.Decrypt(encrypted, true);
            }

            return decrypted;
        }


        //private void GenerateKeys()
        //{
        //    using (var rsa = new RSACryptoServiceProvider(2048))
        //    {
        //        rsa.PersistKeyInCsp = true;
        //        PublicKey = rsa.ExportParameters(false);
        //        PrivateKey = rsa.ExportParameters(true);
        //    }
        //}

        //public byte[] Encrypt(byte[] input)
        //{
        //    byte[] encrypted;
        //    using (var rsa = new RSACryptoServiceProvider(2048))
        //    {
        //        rsa.PersistKeyInCsp = true;
        //        rsa.ImportParameters(PublicKey);
        //        encrypted = rsa.Encrypt(input, true);
        //    }
        //    return encrypted;
        //}

        //public byte[] Decrypt(byte[] input)
        //{
        //    byte[] decrypted;
        //    using (var rsa = new RSACryptoServiceProvider(2048))
        //    {
        //        rsa.PersistKeyInCsp = true;
        //        rsa.ImportParameters(PrivateKey);
        //        decrypted = rsa.Decrypt(input, true);
        //    }
        //    return decrypted;
        //}
    }
}
