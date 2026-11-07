namespace Uber.BLL.ModelVM.Admin
{
    public class EditAdmin
    {

        public string Id { get; set; }
        public string? Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
