namespace Html.PermissionInjector
{
    public class Permission
    {
        public string ResourceName { get; set; }

        public IdentifiedBy IdentifiedBy { get; set; }

        public string Identifier { get; set; }

        public bool HasAccess { get; set; }

    }
}
