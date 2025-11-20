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
public class TransactionsController : ODataController
{
    private readonly BookingVillaPrnContext _context;

    public TransactionsController(BookingVillaPrnContext context)
    {
        _context = context;
    }

    [EnableQuery]
    public IQueryable<Transaction> Get()
    {
        return _context.Transactions
            .Include(t => t.IdTransactionsNavigation)
            .AsNoTracking();
    }

    [EnableQuery]
    public SingleResult<Transaction> Get([FromRoute] int key)
    {
        var result = _context.Transactions.Where(t => t.IdTransactions == key);
        return SingleResult.Create(result);
    }

    public async Task<IActionResult> Post([FromBody] Transaction transaction, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Created(transaction);
    }

    public async Task<IActionResult> Patch([FromRoute] int key, [FromBody] Delta<Transaction> delta, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _context.Transactions.FirstOrDefaultAsync(t => t.IdTransactions == key, cancellationToken);
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
        var existing = await _context.Transactions.FirstOrDefaultAsync(t => t.IdTransactions == key, cancellationToken);
        if (existing == null)
        {
            return NotFound();
        }

        _context.Transactions.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

