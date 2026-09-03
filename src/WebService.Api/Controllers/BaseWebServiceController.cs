using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BaseWebServiceController : ControllerBase
    {
    }
}