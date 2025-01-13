using GameCloud.Application.Features.Functions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Controllers.V1;

[Route("api/v1/[controller]")]
public class FunctionsController(IFunctionService functionService) : BaseController
{
   

}