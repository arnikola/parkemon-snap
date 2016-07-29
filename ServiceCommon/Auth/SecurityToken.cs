namespace ServiceCommon.Auth
{
    using System;

    using Orleans.Concurrency;

    using SimpleCrypto;

    [Immutable]
    [Serializable]
    public class SecurityToken
    {
        /// <summary>
        /// The password hasher.
        /// </summary>
        private static readonly PBKDF2 PasswordHasher = new PBKDF2();

        private string Salt { get; set; }
        private string HashedAuthToken { get; set; }
        
        public SecurityToken(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            this.Salt = PasswordHasher.GenerateSalt(10, 10);
            this.HashedAuthToken = PasswordHasher.Compute(token, this.Salt);
        }

        public bool Authenticate(string token)
        {
            if (token == null)
            {
                return false;
            }

            var hashedAuthToken = PasswordHasher.Compute(token, this.Salt);
            return PasswordHasher.Compare(this.HashedAuthToken, hashedAuthToken);
        }
    }
}