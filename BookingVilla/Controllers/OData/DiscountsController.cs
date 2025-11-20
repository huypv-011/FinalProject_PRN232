using BussinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace BookingVilla.Controllers.OData;

public class DiscountsController : ODataController
{
    private readonly BookingVillaPrnContext _context;

    public DiscountsController(BookingVillaPrnContext context)
    {
        _context = context;
    }

    [EnableQuery]
    [Authorize(Policy = "UserOrAdmin")]
    public IQueryable<Discount> Get()
    {
        return _context.Discounts.AsNoTracking();
    }

    [EnableQuery]
    [Authorize(Policy = "UserOrAdmin")]
    public SingleResult<Discount> Get([FromRoute] int key)
    {
        var result = _context.Discounts.Where(d => d.IdDiscount == key);
        return SingleResult.Create(result);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Post([FromBody] Discount discount, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _context.Discounts.AddAsync(discount, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Created(discount);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Patch([FromRoute] int key, [FromBody] Delta<Discount> delta, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _context.Discounts.FirstOrDefaultAsync(d => d.IdDiscount == key, cancellationToken);
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
        var existing = await _context.Discounts.FirstOrDefaultAsync(d => d.IdDiscount == key, cancellationToken);
        if (existing == null)
        {
            return NotFound();
        }

        _context.Discounts.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

