using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace General.Service
{
    public interface IServiceConfiguration
    {
         string Url { get; set; }
         NetworkCredential Credential { get; set; }
    }
}
