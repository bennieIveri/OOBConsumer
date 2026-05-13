namespace OOB.Data.Entities
{
    public class ApplicationConfig
    {
        public Guid AC_ApplicationID { get; set; }
        public required string AC_KeyName { get; set; }
        public required string AC_Key { get; set; }
        public required string AC_Value { get; set; }

    }
}
