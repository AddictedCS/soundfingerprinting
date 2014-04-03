namespace SoundFingerprinting.Audio.Bass
{
    using System.Configuration;

    public class BassConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("email")]
        public string Email
        {
            get
            {
                return (string)this["email"];
            }

            set
            {
                this["email"] = value;
            }
        }

        [ConfigurationProperty("registrationKey")]
        public string RegistrationKey
        {
            get
            {
                return (string)this["registrationKey"];
            }

            set
            {
                this["registrationKey"] = value;
            }
        }
    }
}
