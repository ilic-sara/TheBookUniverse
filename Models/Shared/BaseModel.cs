using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models.Shared
{
    public class BaseModel
    {
        [ValidateNever]
        public string Id { get; set; } = string.Empty;
    }
}
