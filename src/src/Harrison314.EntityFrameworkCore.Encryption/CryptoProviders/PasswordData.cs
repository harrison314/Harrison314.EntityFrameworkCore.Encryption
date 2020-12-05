namespace Harrison314.EntityFrameworkCore.Encryption.CryptoProviders
{
    internal class PasswordData
    {
        public int Iterations
        {
            get;
            init;
        }

        public byte[] PasswordSalt
        {
            get;
            init;
        }

        public byte[] AesGcmNonce
        {
            get;
            init;
        }

        public byte[] AesGcmTag
        {
            get;
            init;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PasswordData()
        {

        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
