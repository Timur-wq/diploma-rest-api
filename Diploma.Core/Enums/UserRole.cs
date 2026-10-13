using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.Core.Enums
{
    public enum UserRole
    {
        Dispatcher = 1,    // Диспетчер (админ)
        Accountant = 2,    // Бухгалтер
        OmtEmployee = 3,   // Сотрудник ОМТ
        ServiceEngineer = 4, // Сервисный инженер
            Client = 5
    }
}
