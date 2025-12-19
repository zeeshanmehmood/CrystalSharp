using System.Collections.Generic;

namespace CrystalSharp.Application.Handlers
{
    public abstract class Handler
    {
        protected string[] NotFound(params string[] items)
        {
            List<string> errorMessages = [];

            foreach (string item in items)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    errorMessages.Add($"{item} not found");
                }
            }

            return [.. errorMessages];
        }
    }
}
