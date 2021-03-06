using System;
using System.Security.Claims;
using System.Security;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cors;
using CharSheet.Api.Services;
using CharSheet.Api.Models;

namespace CharSheet.Api.Controllers
{
    [ApiController]
    [Route("/api/sheets")]
    public class SheetsController : ControllerBase
    {
        private readonly ILogger<SheetsController> _logger;
        private readonly IBusinessService _service;

        public SheetsController(ILogger<SheetsController> logger, IBusinessService service)
        {
            this._logger = logger;
            this._service = service;
        }

        #region Action Methods
        [HttpGet("{id}")]
        public async Task<ActionResult<SheetModel>> GetSheets(Guid? id)
        {
            try
            {
                // Find by id.
                if (id != null)
                    return Ok(await _service.GetSheet(id));
                return NotFound();
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost("")]
        public async Task<ActionResult<SheetModel>> CreateSheet(SheetModel sheetModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var identity = HttpContext.User.Identity as ClaimsIdentity;
                    var userId = Guid.Parse(identity.Claims.Where(claim => claim.Type == "Id").First().Value);
                    sheetModel = await _service.CreateSheet(sheetModel, userId);
                    return CreatedAtAction(nameof(GetSheets), new { id = sheetModel.SheetId }, sheetModel);
                }
                catch
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SheetModel>> UpdateSheet(Guid? id, SheetModel sheetModel)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                    return BadRequest();
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                var userId = Guid.Parse(identity.Claims.Where(claim => claim.Type == "Id").First().Value);
                sheetModel.SheetId = (Guid)id;
                try
                {
                    return await _service.UpdateSheet(sheetModel, userId);
                }
                catch (SecurityException)
                {
                    return Unauthorized();
                }
                catch
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSheet(Guid? id)
        {
            if (id == null)
                return BadRequest();
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                var userId = Guid.Parse(identity.Claims.Where(claim => claim.Type == "Id").First().Value);
                await _service.DeleteSheet(id, userId);
                return Ok();
            }
            catch (SecurityException)
            {
                return Unauthorized();
            }
            catch
            {
                return BadRequest();
            }
        }
        #endregion
    }
}
