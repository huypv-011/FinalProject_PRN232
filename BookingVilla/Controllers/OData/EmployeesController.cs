using BussinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace BookingVilla.Controllers.OData;

[Authorize(Policy = "AdminOnly")]
public class EmployeesController : ODataController
{
    private readonly BookingVillaPrnContext _context;

    public EmployeesController(BookingVillaPrnContext context)
    {
        _context = context;
    }

    [EnableQuery]
    public IQueryable<Employee> Get()
    {
        return _context.Employees.Include(e => e.IdEmployeeNavigation).AsNoTracking();
    }

    [EnableQuery]
    public SingleResult<Employee> Get([FromRoute] int key)
    {
        var result = _context.Employees.Where(e => e.IdEmployee == key);
        return SingleResult.Create(result);
    }

    public async Task<IActionResult> Post([FromBody] Employee employee, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _context.Employees.AddAsync(employee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Created(employee);
    }

    public async Task<IActionResult> Patch([FromRoute] int key, [FromBody] Delta<Employee> delta, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _context.Employees.FirstOrDefaultAsync(e => e.IdEmployee == key, cancellationToken);
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

    public async Task<IActionResult> Delete([FromRoute] int key, CancellationToken cancellationToken)
    {
        var existing = await _context.Employees.FirstOrDefaultAsync(e => e.IdEmployee == key, cancellationToken);
        if (existing == null)
        {
            return NotFound();
        }

        _context.Employees.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

