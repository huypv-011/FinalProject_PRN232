using System;
using System.Linq;
using BussinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace BookingVilla.Controllers.OData;

public class VillasController : ODataController
{
    private readonly BookingVillaPrnContext _context;

    public VillasController(BookingVillaPrnContext context)
    {
        _context = context;
    }

    [EnableQuery]
    [AllowAnonymous]
    public IQueryable<Villa> Get() => ProjectActiveVillas();

    [EnableQuery]
    [AllowAnonymous]
    public SingleResult<Villa> Get([FromRoute] int key)
    {
        var result = ProjectActiveVillas().Where(v => v.IdVilla == key);
        return SingleResult.Create(result);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Post([FromBody] Villa villa, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _context.Villas.AddAsync(villa, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Created(villa);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Patch([FromRoute] int key, [FromBody] Delta<Villa> delta, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _context.Villas.FirstOrDefaultAsync(v => v.IdVilla == key, cancellationToken);
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
        var existing = await _context.Villas.FirstOrDefaultAsync(v => v.IdVilla == key, cancellationToken);
        if (existing == null)
        {
            return NotFound();
        }

        var hasConflict = await _context.BookingOnlines
            .Include(b => b.CancelBookings)
            .AnyAsync(b =>
                    b.IdVilla == key &&
                    (b.CancelBookings == null || b.CancelBookings.Any(c => c.Status == "Rejected")),
                cancellationToken);

        if (hasConflict)
        {
            return BadRequest("Villa has active bookings.");
        }

        existing.Status = false;
        _context.Villas.Update(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private IQueryable<Villa> ProjectActiveVillas()
    {
        var currentDay = DateTime.Today;

        return _context.Villas
            .Where(v => v.Status == true)
            .Select(v => new Villa
            {
                IdVilla = v.IdVilla,
                Name = v.Name,
                Describe = v.Describe,
                AmountOfPeople = v.AmountOfPeople,
                AmountOfRoom = v.AmountOfRoom,
                Status = v.Status,
                Point = v.Point,
                Price = v.PriceVillas
                    .Where(p => currentDay >= p.FromDate && currentDay <= p.ToDate)
                    .OrderByDescending(p => p.FromDate)
                    .Select(p => p.PriceDay)
                    .FirstOrDefault(),
                ImageVillas = v.ImageVillas
                    .Select(img => new ImageVilla
                    {
                        IdImgVilla = img.IdImgVilla,
                        IdVilla = img.IdVilla,
                        Image = img.Image
                    })
                    .ToList()
            })
            .AsNoTracking();
    }
}

