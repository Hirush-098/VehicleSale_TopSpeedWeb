using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopSpeed.Application.ApplicationConstants
{
    public class ApplicationConstant
    {
    }
    public static class CommonMessage
    {
        public static string RecordCreated = "Record created successfully";
        public static string RecordUpdated = "Record Updated successfully";
        public static string RecordDeleted = "Record Deleted successfully";
    }

    public static class CustomRole
    {
        public const string MasterAdmin = "MASTERADMINS";
        public const string Admin = "ADMINS";
        public const string Customer = "CUSTOMER";

    }
}
