using NagaMaster.Models;
using NagaMaster.Models.User;

namespace NagaMaster.Controllers
{
    internal class ResponseHelper
    {
        public int Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}