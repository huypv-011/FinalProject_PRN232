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
public class CustomersController : ODataController
{
    private readonly BookingVillaPrnContext _context;

    public CustomersController(BookingVillaPrnContext context)
    {
        _context = context;
    }

    [EnableQuery]
    [Authorize(Policy = "AdminOnly")]
    public IQueryable<Customer> Get()
    {
        return _context.Customers
            .Include(c => c.IdCustomerNavigation)
            .AsNoTracking();
    }

    [EnableQuery]
    [Authorize(Policy = "AdminOnly")]
    public SingleResult<Customer> Get([FromRoute] int key)
    {
        var query = _context.Customers.Where(c => c.IdCustomer == key);
        return SingleResult.Create(query);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Post([FromBody] Customer customer, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Created(customer);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Patch([FromRoute] int key, [FromBody] Delta<Customer> delta, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.IdCustomer == key, cancellationToken);
        if (customer == null)
        {
            return NotFound();
        }

        delta.Patch(customer);
        if (!TryValidateModel(customer))
        {
            return BadRequest(ModelState);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Updated(customer);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete([FromRoute] int key, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.IdCustomer == key, cancellationToken);
        if (customer == null)
        {
            return NotFound();
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

