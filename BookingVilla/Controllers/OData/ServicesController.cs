using BussinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace BookingVilla.Controllers.OData;

public class ServicesController : ODataController
{
    private readonly BookingVillaPrnContext _context;

    public ServicesController(BookingVillaPrnContext context)
    {
        _context = context;
    }

    [EnableQuery]
    [AllowAnonymous]
    public IQueryable<Service> Get()
    {
        return _context.Services.AsNoTracking();
    }

    [EnableQuery]
    [AllowAnonymous]
    public SingleResult<Service> Get([FromRoute] int key)
    {
        var result = _context.Services.Where(s => s.IdService == key);
        return SingleResult.Create(result);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Post([FromBody] Service service, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _context.Services.AddAsync(service, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Created(service);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Patch([FromRoute] int key, [FromBody] Delta<Service> delta, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _context.Services.FirstOrDefaultAsync(s => s.IdService == key, cancellationToken);
        if (existing == null)
        {
            return NotFound();
        }

        delta.Patch(existing);
        if (!TryValidateModel(existing))
        {
            return BadRequest(ModelState);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Updated(existing);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete([FromRoute] int key, CancellationToken cancellationToken)
    {
        var existing = await _context.Services.FirstOrDefaultAsync(s => s.IdService == key, cancellationToken);
        if (existing == null)
        {
            return NotFound();
        }

        _context.Services.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

