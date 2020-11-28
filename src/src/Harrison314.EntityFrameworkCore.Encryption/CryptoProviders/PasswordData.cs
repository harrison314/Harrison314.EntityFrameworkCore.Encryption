namespace Harrison314.EntityFrameworkCore.Encryption.CryptoProviders
{
    internal class PasswordData
    {
        public int Iterations
        {
            get;
            set;
        }

        public byte[] PasswordSalt
        {
            get;
            set;
        }

        public byte[] AesGcmNonce
        {
            get;
            set;
        }

        public byte[] AesGcmTag
        {
            get;
            set;
        }

        public PasswordData()
        {

        }
    }
}
