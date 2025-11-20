using BussinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace BookingVilla.Controllers.OData;

[Authorize]
public class BookingOnlinesController : ODataController
{
    private readonly BookingVillaPrnContext _context;

    public BookingOnlinesController(BookingVillaPrnContext context)
    {
        _context = context;
    }

    [EnableQuery]
    [Authorize(Policy = "UserOrAdmin")]
    public IQueryable<BookingOnline> Get()
    {
        return _context.BookingOnlines
            .Include(b => b.IdCustomerNavigation)
            .Include(b => b.IdVillaNavigation)
            .AsNoTracking();
    }

    [EnableQuery]
    [Authorize(Policy = "UserOrAdmin")]
    public SingleResult<BookingOnline> Get([FromRoute] int key)
    {
        var result = _context.BookingOnlines
            .Where(b => b.IdBookingOnline == key);

        return SingleResult.Create(result);
    }

    [Authorize(Policy = "UserOrAdmin")]
    public async Task<IActionResult> Post([FromBody] BookingOnline booking, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _context.BookingOnlines.AddAsync(booking, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Created(booking);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Patch([FromRoute] int key, [FromBody] Delta<BookingOnline> delta, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _context.BookingOnlines.FirstOrDefaultAsync(b => b.IdBookingOnline == key, cancellationToken);
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
        var existing = await _context.BookingOnlines.FirstOrDefaultAsync(b => b.IdBookingOnline == key, cancellationToken);
        if (existing == null)
        {
            return NotFound();
        }

        _context.BookingOnlines.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

