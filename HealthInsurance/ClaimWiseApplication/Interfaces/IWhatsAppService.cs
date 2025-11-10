using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimWise.Application.Interfaces
{
    public interface IWhatsAppService
    {
        Task SendMessageAsync(string toPhoneNumber, string message);
    }

}
